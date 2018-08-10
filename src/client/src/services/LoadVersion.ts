import { VersionInfo } from "src/models/version";

async function FetchVersion(): Promise<VersionInfo> {
    return fetch("/data/version.json")
    .then(response => {
      if (!response.ok) {
        throw new Error(response.statusText)
      }
      return response.json();
    }).catch((e: Error) => {
        const ver: VersionInfo = {
            built: 0,
            version: 'error'
        };
        return ver;
    });
}

export { FetchVersion }