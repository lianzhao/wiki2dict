export default async function retry<T>(func: (attempts: number) => Promise<T>, retry = 1) {
  let attempts = 0;
  let error: Error;
  do {
    try {
      return await func(attempts);
    } catch (e) {
      error = e as any;
      attempts++;
    }
  } while (attempts <= retry);
  throw error;
}
