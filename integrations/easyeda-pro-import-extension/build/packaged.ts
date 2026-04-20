import path from 'node:path';
import fs from 'fs-extra';
import ignore from 'ignore';
import JSZip from 'jszip';

import * as extensionConfig from '../extension.json';

function multiLineStrToArray(value: string): Array<string> {
  return value.split(/[\r\n]+/);
}

function testUuid(uuid?: string): uuid is string {
  const regExp = /^[a-z0-9]{32}$/;
  return !!uuid && uuid !== '00000000000000000000000000000000' && regExp.test(uuid.trim());
}

function fixUuid(uuid?: string): string {
  return testUuid(uuid) ? uuid.trim() : crypto.randomUUID().replaceAll('-', '');
}

function main(): void {
  if (!testUuid(extensionConfig.uuid)) {
    const nextConfig = { ...extensionConfig, uuid: fixUuid(extensionConfig.uuid) };
    fs.writeJsonSync(path.join(__dirname, '../extension.json'), nextConfig, { spaces: '\t', EOL: '\n', encoding: 'utf-8' });
  }

  const filepathListWithoutFilter = fs.readdirSync(path.join(__dirname, '../'), { encoding: 'utf-8', recursive: true });
  const ignorePatterns = multiLineStrToArray(fs.readFileSync(path.join(__dirname, '../.edaignore'), { encoding: 'utf-8' }));
  const effectivePatterns = ignorePatterns.map(pattern => pattern.endsWith('/') || pattern.endsWith('\\') ? pattern.slice(0, -1) : pattern);
  const edaIgnore = ignore().add(effectivePatterns);
  const filepathListWithoutResolve = edaIgnore.filter(filepathListWithoutFilter);
  const fileList: Array<string> = [];

  for (const filepath of filepathListWithoutResolve) {
    if (fs.lstatSync(filepath).isFile()) {
      fileList.push(filepath.replace(/\\/g, '/'));
    }
  }

  const zip = new JSZip();
  for (const file of fileList) {
    zip.file(file, fs.createReadStream(path.join(__dirname, '../', file)));
  }

  zip.generateNodeStream({ type: 'nodebuffer', streamFiles: true, compression: 'DEFLATE', compressionOptions: { level: 9 } }).pipe(
    fs.createWriteStream(path.join(__dirname, 'dist', `${extensionConfig.name}_v${extensionConfig.version}.eext`))
  );
}

main();
