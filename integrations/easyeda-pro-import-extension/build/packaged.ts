import path from 'node:path';
import { fileURLToPath } from 'node:url';
import fs from 'fs-extra';
import ignore from 'ignore';
import JSZip from 'jszip';

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
  const scriptDir = path.dirname(fileURLToPath(import.meta.url));
  const projectRoot = path.resolve(scriptDir, '..');
  const extensionConfigPath = path.join(projectRoot, 'extension.json');
  const extensionConfig = fs.readJsonSync(extensionConfigPath) as Record<string, string>;
  const outputDir = path.join(scriptDir, 'dist');

  fs.ensureDirSync(outputDir);

  if (!testUuid(extensionConfig.uuid)) {
    const nextConfig = { ...extensionConfig, uuid: fixUuid(extensionConfig.uuid) };
    fs.writeJsonSync(extensionConfigPath, nextConfig, { spaces: '\t', EOL: '\n', encoding: 'utf-8' });
  }

  const filepathListWithoutFilter = fs.readdirSync(projectRoot, { encoding: 'utf-8', recursive: true });
  const ignorePatterns = multiLineStrToArray(fs.readFileSync(path.join(projectRoot, '.edaignore'), { encoding: 'utf-8' }));
  const effectivePatterns = ignorePatterns.map(pattern => pattern.endsWith('/') || pattern.endsWith('\\') ? pattern.slice(0, -1) : pattern);
  const edaIgnore = ignore().add(effectivePatterns);
  const filepathListWithoutResolve = edaIgnore.filter(filepathListWithoutFilter);
  const fileList: Array<string> = [];

  for (const filepath of filepathListWithoutResolve) {
    if (fs.lstatSync(path.join(projectRoot, filepath)).isFile()) {
      fileList.push(filepath.replace(/\\/g, '/'));
    }
  }

  const zip = new JSZip();
  for (const file of fileList) {
    zip.file(file, fs.createReadStream(path.join(projectRoot, file)));
  }

  zip.generateNodeStream({ type: 'nodebuffer', streamFiles: true, compression: 'DEFLATE', compressionOptions: { level: 9 } }).pipe(
    fs.createWriteStream(path.join(outputDir, `${extensionConfig.name}_v${extensionConfig.version}.eext`))
  );
}

main();
