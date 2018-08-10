import { Company } from "src/models/company";
import { Board } from 'src/models/board';

function GetCommonCompanies() : Promise<Array<Company>> {
    return fetch("/data/common_companies.json")
    .then(response => {
      if (!response.ok) {
        throw new Error(response.statusText);
      }
      return response.json();
    })
}

function GetCommonBoards() : Promise<Array<Board>> {
    return fetch("/data/common_boards.json")
    .then(response => {
      if (!response.ok) {
        throw new Error(response.statusText);
      }
      return response.json();
    })
}


export { GetCommonCompanies, GetCommonBoards }