import * as extensionConfig from '../extension.json';
import {
  extractDocumentLinks,
  normalizeEasyEdaPayload,
  type DeviceItem,
  type Model3DItem,
  type SearchItem,
  stringifyRaw
} from './import-helpers';

declare const eda: any;

interface ConnectorSettings {
  backendBaseUrl: string;
  importApiKey: string;
  defaultLibraryUuid?: string;
}

const SETTINGS_KEY = 'cadence-cis-admin.easyeda-import.settings';

export function activate(): void {}

export function about(): void {
  alert(`${extensionConfig.displayName} v${extensionConfig.version}\nExports EasyEDA Pro library data into CadenceComponentLibraryAdmin staging.`);
}

export async function configureConnector(): Promise<void> {
  const existing = loadSettings();
  const backendBaseUrl = prompt('Cadence CIS Admin backend base URL', existing.backendBaseUrl || 'http://localhost:8080');
  if (!backendBaseUrl) {
    return;
  }

  const importApiKey = prompt('X-Import-Api-Key', existing.importApiKey || '');
  if (!importApiKey) {
    return;
  }

  const defaultLibraryUuid = prompt('Default EasyEDA library UUID (optional)', existing.defaultLibraryUuid || '') || undefined;
  saveSettings({ backendBaseUrl, importApiKey, defaultLibraryUuid });
  alert('Connector settings saved.');
}

export async function exportComponentToCisAdmin(): Promise<void> {
  const keyword = prompt('Search keyword');
  if (!keyword) {
    return;
  }

  await exportBySearch(async settings => {
    const deviceApi = getDeviceApi();
    return await deviceApi.search(keyword, settings.defaultLibraryUuid || undefined);
  }, `keyword "${keyword}"`);
}

export async function exportComponentByLcsc(): Promise<void> {
  const lcscId = prompt('LCSC C-number (for example C2040)');
  if (!lcscId) {
    return;
  }

  await exportBySearch(async settings => {
    const deviceApi = getDeviceApi();
    const response = await deviceApi.getByLcscIds(lcscId, settings.defaultLibraryUuid || undefined, false);
    return Array.isArray(response) ? response : response ? [response] : [];
  }, `LCSC "${lcscId}"`);
}

export async function exportCurrentSelection(): Promise<void> {
  alert('Current-selection import is only best-effort in B3. The public SDK references reviewed for this milestone did not confirm a stable selected-library-device API, so use keyword or LCSC search if this entry does not work in your runtime.');
}

async function exportBySearch(
  searchFn: (settings: ConnectorSettings) => Promise<Array<SearchItem>>,
  label: string
): Promise<void> {
  const settings = ensureSettings();
  const results = await searchFn(settings);
  if (!results || results.length === 0) {
    alert(`No devices found for ${label}.`);
    return;
  }

  const selection = prompt(renderPreviewPrompt(results));
  if (!selection) {
    return;
  }

  const selectedIndex = Number.parseInt(selection, 10);
  if (Number.isNaN(selectedIndex) || selectedIndex < 1 || selectedIndex > results.length) {
    alert('Selection index is invalid.');
    return;
  }

  const searchItem = results[selectedIndex - 1];
  const payload = await collectPayload(searchItem, settings);
  const importResponse = await postImport(payload, settings);

  if (payload.footprintRenderImage) {
    await uploadAsset(importResponse.importId, 'FootprintRenderImage', payload.footprintRenderImage, payload.footprintRenderMetadata, settings);
  }

  await uploadUrlAssetIfPresent(importResponse.importId, 'Datasheet', payload.datasheetUrl, payload.documentLinkMetadata?.datasheet, settings);
  await uploadUrlAssetIfPresent(importResponse.importId, 'Manual', payload.manualUrl, payload.documentLinkMetadata?.manual, settings);
  await uploadUrlAssetIfPresent(importResponse.importId, 'Step', payload.stepUrl, payload.documentLinkMetadata?.step, settings);

  alert(`Import completed.\nImport ID: ${importResponse.importId}\nWarnings: ${(importResponse.duplicateWarnings || []).join('; ') || 'None'}`);
}

async function collectPayload(searchItem: SearchItem, settings: ConnectorSettings): Promise<any> {
  const deviceApi = getDeviceApi();
  const footprintApi = getFootprintApi();
  const model3DApi = get3DModelApi();

  const deviceItem = searchItem.uuid ? await deviceApi.get(searchItem.uuid, searchItem.libraryUuid || settings.defaultLibraryUuid || undefined) : undefined;
  const association = deviceItem?.association ?? searchItem?.association ?? null;
  const footprintUuid = searchItem.footprint?.uuid || searchItem.footprintUuid || association?.footprint?.uuid;
  const footprintLibraryUuid = searchItem.footprint?.libraryUuid || association?.footprint?.libraryUuid || searchItem.libraryUuid || settings.defaultLibraryUuid;
  const footprintItem = footprintUuid && footprintApi.get ? await footprintApi.get(footprintUuid, footprintLibraryUuid) : undefined;

  const model3DUuid = searchItem.model3D?.uuid || searchItem.model3DUuid || association?.model3D?.uuid;
  const model3DLibraryUuid = searchItem.model3D?.libraryUuid || association?.model3D?.libraryUuid || searchItem.libraryUuid || settings.defaultLibraryUuid;
  const model3DItem = model3DUuid && model3DApi.get ? await model3DApi.get(model3DUuid, model3DLibraryUuid) : undefined;

  const documentLinks = extractDocumentLinks(searchItem, deviceItem, model3DItem);
  const footprintRenderImage = footprintUuid && footprintApi.getRenderImage
    ? await tryGetRenderImage(footprintApi, footprintUuid, footprintLibraryUuid)
    : undefined;

  const normalized = normalizeEasyEdaPayload({
    searchItem,
    deviceItem,
    footprintItem,
    model3DItem,
    documentLinks,
    defaultLibraryUuid: settings.defaultLibraryUuid
  });

  return {
    ...normalized,
    footprintRenderImage,
    footprintRenderMetadata: footprintRenderImage ? { footprintUuid, footprintLibraryUuid } : undefined,
    documentLinkMetadata: {
      datasheet: documentLinks.datasheetUrl ? { source: 'heuristic' } : undefined,
      manual: documentLinks.manualUrl ? { source: 'heuristic' } : undefined,
      step: documentLinks.stepUrl ? documentLinks.stepMetadata || { source: 'heuristic' } : undefined
    }
  };
}

function renderPreviewPrompt(results: Array<SearchItem>): string {
  const lines = results.slice(0, 10).map((item, index) => {
    const manufacturer = item.manufacturer || item.property?.manufacturer || 'Unknown';
    const footprint = item.footprint?.name || item.footprintName || '-';
    return `${index + 1}. ${item.name || 'Unnamed'} | ${manufacturer} | ${item.uuid} | ${footprint}`;
  });

  return `Select a device to export by index:\n\n${lines.join('\n')}`;
}

async function postImport(payload: any, settings: ConnectorSettings): Promise<any> {
  const response = await fetch(`${trimSlash(settings.backendBaseUrl)}/api/import/easyeda/component`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Import-Api-Key': settings.importApiKey
    },
    body: JSON.stringify(stripTransportOnlyFields(payload))
  });

  if (!response.ok) {
    throw new Error(`Import failed with status ${response.status}`);
  }

  return await response.json();
}

async function uploadAsset(
  importId: number,
  assetType: string,
  asset: string | Blob,
  rawMetadata: Record<string, any> | undefined,
  settings: ConnectorSettings
): Promise<void> {
  const formData = new FormData();
  formData.append('assetType', assetType);
  formData.append('rawMetadataJson', stringifyRaw(rawMetadata || null));

  if (typeof asset === 'string') {
    if (asset.startsWith('data:')) {
      const blob = await (await fetch(asset)).blob();
      formData.append('file', blob, `${assetType.toLowerCase()}.png`);
    } else {
      formData.append('url', asset);
    }
  } else {
    formData.append('file', asset, `${assetType.toLowerCase()}.bin`);
  }

  const response = await fetch(`${trimSlash(settings.backendBaseUrl)}/api/import/easyeda/component/${importId}/asset`, {
    method: 'POST',
    headers: {
      'X-Import-Api-Key': settings.importApiKey
    },
    body: formData
  });

  if (!response.ok) {
    throw new Error(`Asset upload failed with status ${response.status}`);
  }
}

async function uploadUrlAssetIfPresent(
  importId: number,
  assetType: string,
  url: string | undefined,
  rawMetadata: Record<string, any> | undefined,
  settings: ConnectorSettings
): Promise<void> {
  if (!url) {
    return;
  }

  const formData = new FormData();
  formData.append('assetType', assetType);
  formData.append('url', url);
  formData.append('rawMetadataJson', stringifyRaw(rawMetadata || null));

  const response = await fetch(`${trimSlash(settings.backendBaseUrl)}/api/import/easyeda/component/${importId}/asset`, {
    method: 'POST',
    headers: {
      'X-Import-Api-Key': settings.importApiKey
    },
    body: formData
  });

  if (!response.ok) {
    throw new Error(`URL asset upload failed with status ${response.status}`);
  }
}

async function tryGetRenderImage(footprintApi: any, footprintUuid: string, libraryUuid?: string): Promise<string | Blob | undefined> {
  try {
    return await footprintApi.getRenderImage(footprintUuid, libraryUuid);
  } catch {
    return undefined;
  }
}

function stripTransportOnlyFields(payload: Record<string, any>): Record<string, any> {
  const { footprintRenderImage, footprintRenderMetadata, documentLinkMetadata, ...rest } = payload;
  return rest;
}

function getDeviceApi(): any {
  return eda?.LIB_Device ?? eda?.lib_Device ?? eda?.LIB?.Device;
}

function getFootprintApi(): any {
  return eda?.LIB_Footprint ?? eda?.lib_Footprint ?? eda?.LIB?.Footprint ?? {};
}

function get3DModelApi(): any {
  return eda?.LIB_3DModel ?? eda?.lib_3DModel ?? eda?.LIB?.Model3D ?? {};
}

function loadSettings(): ConnectorSettings {
  const rawValue = localStorage.getItem(SETTINGS_KEY);
  if (!rawValue) {
    return {
      backendBaseUrl: 'http://localhost:8080',
      importApiKey: ''
    };
  }

  try {
    return JSON.parse(rawValue) as ConnectorSettings;
  } catch {
    return {
      backendBaseUrl: 'http://localhost:8080',
      importApiKey: ''
    };
  }
}

function saveSettings(settings: ConnectorSettings): void {
  localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
}

function ensureSettings(): ConnectorSettings {
  const settings = loadSettings();
  if (!settings.backendBaseUrl || !settings.importApiKey) {
    throw new Error('Connector settings are incomplete. Run Configure Connector first.');
  }

  return settings;
}

function trimSlash(value: string): string {
  return value.replace(/\/+$/, '');
}
