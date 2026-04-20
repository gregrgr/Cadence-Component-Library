import assert from 'node:assert/strict';
import { extractDocumentLinks, normalizeEasyEdaPayload } from '../src/import-helpers.ts';

const searchItem = {
  uuid: 'dev-001',
  libraryUuid: 'lib-001',
  name: 'LM358',
  description: 'Dual op-amp',
  manufacturer: 'ACME',
  supplier: 'LCSC',
  supplierId: 'C2040',
  otherProperty: {
    DataSheet: 'https://example.test/lm358-datasheet.pdf',
    unknownField: 'keep-me'
  },
  symbol: {
    uuid: 'sym-001',
    name: 'LM358_SYM',
    libraryUuid: 'sym-lib-001'
  },
  footprint: {
    uuid: 'fpt-001',
    name: 'SOIC-8',
    libraryUuid: 'fpt-lib-001'
  },
  model3D: {
    uuid: '3d-001',
    name: 'SOIC-8-3D',
    libraryUuid: '3d-lib-001'
  },
  imageUuids: ['img-001', 'img-001', 'img-002']
};

const deviceItem = {
  name: 'LM358 DEVICE',
  property: {
    manufacturer: 'ACME',
    manufacturerPN: 'LM358-ACME',
    name: 'LM358-ACME',
    otherProperty: {
      ManualUrl: 'https://example.test/lm358-manual.pdf',
      STEP: 'https://example.test/lm358.step',
      mysteryProperty: 'do-not-drop'
    }
  },
  association: {
    symbol: {
      uuid: 'sym-001',
      name: 'LM358_SYM'
    },
    footprint: {
      uuid: 'fpt-001',
      name: 'SOIC-8'
    }
  }
};

const model3DItem = {
  uuid: '3d-001',
  downloadUrl: 'https://example.test/lm358-model.step'
};

const links = extractDocumentLinks(searchItem, deviceItem, model3DItem);
assert.equal(links.datasheetUrl, 'https://example.test/lm358-datasheet.pdf');
assert.equal(links.manualUrl, 'https://example.test/lm358-manual.pdf');
assert.equal(links.stepUrl, 'https://example.test/lm358.step');

const normalized = normalizeEasyEdaPayload({
  searchItem,
  deviceItem,
  model3DItem,
  footprintItem: { uuid: 'fpt-001', name: 'SOIC-8' },
  documentLinks: links,
  defaultLibraryUuid: 'lib-001'
});

assert.equal(normalized.externalDeviceUuid, 'dev-001');
assert.equal(normalized.manufacturer, 'ACME');
assert.equal(normalized.manufacturerPN, 'LM358-ACME');
assert.deepEqual(normalized.imageUuids, ['img-001', 'img-002']);
assert.match(normalized.otherPropertyRawJson, /mysteryProperty/);
assert.match(normalized.searchItemRawJson, /unknownField/);

console.log('EasyEDA extension sanity-check passed.');
