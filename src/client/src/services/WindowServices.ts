function ToQueryString(m: Map<string, string>): string {
    let s: string = "";
    const sarr: string[] = [];
    if(m.size > 0) {
      s += "?";
      m.forEach((v,k) => {
        sarr.push(k+"="+v);
      });
      s += sarr.join("&");
    }
    return s;
  }

  
function ParseQueryString(q: string | undefined): Map<string, string> {
  console.log(q);
    const m: Map<string, string> = new Map<string, string>();
    if(q) {
      q.substr(1).split('&').forEach((val, idx, arr) => {
        let firstEq = val.indexOf('=');
        if(firstEq == -1) {
          return;
        }
        if(firstEq+1 > val.length) {
          return;
        }
        const key = val.substr(0, firstEq);
        const value = val.substr(firstEq+1, val.length-firstEq);
        if(key !== "" && value !== "") {
          m.set(key, decodeURI(value));
        }
      });
    }
    return m;
  }
  
  // https://eureka.ykyuen.info/2015/04/08/javascript-add-query-parameter-to-current-url-without-reload/
  function UpdateURL(key: string | null, val: string[] | null) {
    if (history.pushState) {
        var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname;
        if(key !== null && val !== null && val.length > 0) {
          const v:string = val.join(',');
          const x = ParseQueryString(window.location.search);
          x.set(key, v);
          const qs: string = ToQueryString(x);
          newurl += qs;
        }
        if(key !== null && val == null) {
          // kill the key entirely
          const x = ParseQueryString(window.location.search);
          x.delete(key);
          const qs: string = ToQueryString(x);
          newurl += qs;
        }
        window.history.replaceState({path:newurl},'',newurl);
    }
  }

  export { ToQueryString, UpdateURL, ParseQueryString }