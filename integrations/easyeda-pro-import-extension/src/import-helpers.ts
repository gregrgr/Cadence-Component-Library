export type SearchItem = Record<string, any>;
export type DeviceItem = Record<string, any>;
export type Model3DItem = Record<string, any>;

export interface DocumentLinks {
  datasheetUrl?: string;
  manualUrl?: string;
  stepUrl?: string;
  stepMetadata?: Record<string, any>;
}

export interface NormalizedEasyEdaPayload {
  sourceName: string;
  externalDeviceUuid?: string;
  externalLibraryUuid?: string;
  searchKeyword: string;
  lcscId?: string;
  name?: string;
  description?: string;
  classification?: unknown;
  manufacturer?: string;
  manufacturerPN?: string;
  supplier?: string;
  supplierId?: string;
  symbolName?: string;
  symbolUuid?: string;
  symbolLibraryUuid?: string;
  symbolType?: string;
  symbolRawJson: string;
  footprintName?: string;
  footprintUuid?: string;
  footprintLibraryUuid?: string;
  footprintRawJson: string;
  imageUuids: Array<string>;
  model3DName?: string;
  model3DUuid?: string;
  model3DLibraryUuid?: string;
  model3DRawJson: string;
  datasheetUrl?: string;
  manualUrl?: string;
  stepUrl?: string;
  jlcInventory?: number;
  jlcPrice?: number;
  lcscInventory?: number;
  lcscPrice?: number;
  searchItemRawJson: string;
  deviceItemRawJson: string;
  deviceAssociationRawJson: string;
  devicePropertyRawJson: string;
  otherPropertyRawJson: string;
  fullRawJson: string;
}

export function normalizeEasyEdaPayload(args: {
  searchItem: SearchItem;
  deviceItem?: DeviceItem;
  footprintItem?: Record<string, any>;
  model3DItem?: Model3DItem;
  documentLinks: DocumentLinks;
  defaultLibraryUuid?: string;
}): NormalizedEasyEdaPayload {
  const { searchItem, deviceItem, footprintItem, model3DItem, documentLinks, defaultLibraryUuid } = args;
  const association = deviceItem?.association ?? searchItem?.association ?? null;
  const property = deviceItem?.property ?? null;

  const footprintUuid = searchItem.footprint?.uuid || searchItem.footprintUuid || association?.footprint?.uuid;
  const footprintLibraryUuid = searchItem.footprint?.libraryUuid || association?.footprint?.libraryUuid || searchItem.libraryUuid || defaultLibraryUuid;
  const model3DUuid = searchItem.model3D?.uuid || searchItem.model3DUuid || association?.model3D?.uuid;
  const model3DLibraryUuid = searchItem.model3D?.libraryUuid || association?.model3D?.libraryUuid || searchItem.libraryUuid || defaultLibraryUuid;

  return {
    sourceName: 'EasyEDA Pro',
    externalDeviceUuid: searchItem.uuid,
    externalLibraryUuid: searchItem.libraryUuid || defaultLibraryUuid || undefined,
    searchKeyword: searchItem.name || searchItem.description || '',
    lcscId: findLcscId(searchItem, deviceItem),
    name: searchItem.name || deviceItem?.name || undefined,
    description: searchItem.description || deviceItem?.description || undefined,
    classification: searchItem.classification || deviceItem?.classification || undefined,
    manufacturer: searchItem.manufacturer || property?.manufacturer || undefined,
    manufacturerPN: property?.name || deviceItem?.manufacturerPN || undefined,
    supplier: searchItem.supplier || property?.supplier || undefined,
    supplierId: searchItem.supplierId || property?.supplierId || undefined,
    symbolName: searchItem.symbol?.name || searchItem.symbolName || association?.symbol?.name || undefined,
    symbolUuid: searchItem.symbol?.uuid || searchItem.symbolUuid || association?.symbol?.uuid || undefined,
    symbolLibraryUuid: searchItem.symbol?.libraryUuid || association?.symbol?.libraryUuid || undefined,
    symbolType: deviceItem?.symbolType || association?.symbolType || undefined,
    symbolRawJson: stringifyRaw(searchItem.symbol || association?.symbol || null),
    footprintName: searchItem.footprint?.name || searchItem.footprintName || association?.footprint?.name || footprintItem?.name || undefined,
    footprintUuid,
    footprintLibraryUuid,
    footprintRawJson: stringifyRaw(footprintItem || association?.footprint || null),
    imageUuids: dedupeStrings([
      searchItem.imageUuid,
      ...(asArray(searchItem.imageUuids))
    ]),
    model3DName: searchItem.model3D?.name || searchItem.model3DName || association?.model3D?.name || model3DItem?.name || undefined,
    model3DUuid,
    model3DLibraryUuid,
    model3DRawJson: stringifyRaw(model3DItem || association?.model3D || null),
    datasheetUrl: documentLinks.datasheetUrl,
    manualUrl: documentLinks.manualUrl,
    stepUrl: documentLinks.stepUrl,
    jlcInventory: searchItem.jlcInventory,
    jlcPrice: searchItem.jlcPrice,
    lcscInventory: searchItem.lcscInventory,
    lcscPrice: searchItem.lcscPrice,
    searchItemRawJson: stringifyRaw(searchItem),
    deviceItemRawJson: stringifyRaw(deviceItem || null),
    deviceAssociationRawJson: stringifyRaw(association),
    devicePropertyRawJson: stringifyRaw(property),
    otherPropertyRawJson: stringifyRaw(property?.otherProperty || searchItem.otherProperty || null),
    fullRawJson: stringifyRaw({ searchItem, deviceItem, footprintItem, model3DItem })
  };
}

export function findLcscId(searchItem: SearchItem, deviceItem: DeviceItem | undefined): string | undefined {
  const directValue = searchItem.lcscId || deviceItem?.lcscId;
  if (typeof directValue === 'string' && directValue.trim()) {
    return directValue.trim();
  }

  const matches = findValuesByLikelyKeys({
    searchItem,
    deviceItem,
    property: deviceItem?.property,
    association: deviceItem?.association
  }, ['lcsc', 'cnumber', 'c-number', 'supplierid']);

  return matches.find(value => /^c\d+$/i.test(value));
}

export function extractDocumentLinks(searchItem?: SearchItem, deviceItem?: DeviceItem, model3DItem?: Model3DItem): DocumentLinks {
  const root = {
    searchItem,
    deviceItem,
    model3DItem,
    searchOtherProperty: searchItem?.otherProperty,
    deviceOtherProperty: deviceItem?.property?.otherProperty,
    deviceProperty: deviceItem?.property,
    deviceAssociation: deviceItem?.association
  };

  const urlEntries = findUrlEntries(root);
  const result: DocumentLinks = {};

  for (const entry of urlEntries) {
    const key = entry.key.toLowerCase();
    if (!result.datasheetUrl && matchesAny(key, ['datasheet', 'datasheeturl', 'datasheet_url', 'pdf', 'document', 'spec', 'specification'])) {
      result.datasheetUrl = entry.value;
      continue;
    }

    if (!result.manualUrl && matchesAny(key, ['manual', 'manualurl', 'manual_url'])) {
      result.manualUrl = entry.value;
      continue;
    }

    if (!result.stepUrl && matchesAny(key, ['step', 'stepurl', 'step_url', 'stp', '3d', 'model3d', 'model3durl', 'model'])) {
      result.stepUrl = entry.value;
      result.stepMetadata = entry.parent;
    }
  }

  return result;
}

export function stringifyRaw(value: unknown): string {
  return JSON.stringify(value ?? null, null, 2);
}

export function dedupeStrings(values: Array<unknown>): Array<string> {
  return [...new Set(values.filter((value): value is string => typeof value === 'string' && value.trim().length > 0).map(value => value.trim()))];
}

function findUrlEntries(root: unknown): Array<{ key: string; value: string; parent: Record<string, any> }> {
  const matches: Array<{ key: string; value: string; parent: Record<string, any> }> = [];
  const seen = new WeakSet<object>();

  function visit(value: unknown): void {
    if (!value || typeof value !== 'object') {
      return;
    }

    if (seen.has(value as object)) {
      return;
    }

    seen.add(value as object);

    if (Array.isArray(value)) {
      for (const item of value) {
        visit(item);
      }
      return;
    }

    for (const [key, nested] of Object.entries(value as Record<string, any>)) {
      if (typeof nested === 'string' && looksLikeUrl(nested)) {
        matches.push({ key, value: nested, parent: value as Record<string, any> });
      } else {
        visit(nested);
      }
    }
  }

  visit(root);
  return matches;
}

function findValuesByLikelyKeys(root: unknown, candidates: Array<string>): Array<string> {
  const found: Array<string> = [];
  const seen = new WeakSet<object>();

  function visit(value: unknown): void {
    if (!value || typeof value !== 'object') {
      return;
    }

    if (seen.has(value as object)) {
      return;
    }

    seen.add(value as object);

    if (Array.isArray(value)) {
      for (const item of value) {
        visit(item);
      }
      return;
    }

    for (const [key, nested] of Object.entries(value as Record<string, any>)) {
      const normalizedKey = key.toLowerCase();
      if (typeof nested === 'string' && candidates.some(candidate => normalizedKey.includes(candidate.toLowerCase()))) {
        found.push(nested);
      } else {
        visit(nested);
      }
    }
  }

  visit(root);
  return found;
}

function looksLikeUrl(value: string): boolean {
  return /^https?:\/\//i.test(value.trim());
}

function matchesAny(value: string, candidates: Array<string>): boolean {
  return candidates.some(candidate => value.includes(candidate.toLowerCase()));
}

function asArray(value: unknown): Array<unknown> {
  return Array.isArray(value) ? value : [];
}
