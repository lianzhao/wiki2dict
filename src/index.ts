import fs from 'fs/promises';
import run from './run';

const url = process.argv[2];
try {
  const { siteInfo, data } = await run(url, { onMessage: e => console[e.level](e.message, e.helpLink) });
  await fs.writeFile(`${siteInfo.name}_dict.zip`, data);
} catch (e) {
  console.error(e);
}
