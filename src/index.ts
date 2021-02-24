import fs from 'fs/promises';
import winston from 'winston';
import run from './run';

const url = process.argv[2];
const logger = winston.createLogger({
  transports: [
    new winston.transports.Console({
      format: winston.format.combine(winston.format.cli()),
    }),
  ],
});
try {
  const { siteInfo, data } = await run(url, { onMessage: e => logger[e.level](e.message, e.helpLink) });
  await fs.writeFile(`${siteInfo.name}_dict.zip`, data);
} catch (e) {
  logger.error(e);
}
