import fs from 'fs/promises';
import Runner, { DoneEvent, MessageEvent, ErrorEvent } from './runner';

function handleRunnerMessage(e: MessageEvent) {
  console[e.level](e.message, e.helpLink);
}

function handleRunnerDone(e: DoneEvent) {
  removeRunnerEventListeners(e.target as Runner);
  fs.writeFile(`${e.siteInfo.name}_dict.zip`, e.data).catch(e => {
    console.error(e);
  });
  // saveAs(new Blob([e.data], { type: 'application/zip' }), `${e.siteInfo.name}_dict.zip`);
}

function handleRunnerError(e: ErrorEvent) {
  removeRunnerEventListeners(e.target as Runner);
}

function removeRunnerEventListeners(runner: Runner) {
  runner.removeEventListener('message', handleRunnerMessage as any);
  runner.removeEventListener('done', handleRunnerDone as any);
  runner.removeEventListener('error', handleRunnerError as any);
}

const url = 'https://princeofnothing.fandom.com/wiki/Prince_of_Nothing_Wiki';
const runner = new Runner(url);
runner.addEventListener('message', handleRunnerMessage as any);
runner.addEventListener('done', handleRunnerDone as any);
runner.addEventListener('error', handleRunnerError as any);
runner.run();
