import * as React from 'react';
import {
   GetBuySellTradeThreadData, ParseData
  } from 'src/services/GetNewestData';
import './App.css';
import { GetCommonCompanies, GetCommonBoards } from 'src/services/GetCommonCompanies';
import { Company } from 'src/models/company';
import { IRedditData } from 'src/models/reddit';
import { Board } from 'src/models/board';
import { AugmentCompanies } from 'src/services/DispalyMappers';
import { IBSTThread, IBSTThreadComment, DefautThread } from 'src/models/IBST';
import { IBSTError } from 'src/models/BSTErrors';
import { BSTComment } from 'src/react/components/BSTComment/BSTComment';
import { BSTFooter } from 'src/react/components/BSTFooter/BSTFooter';
import { BSTHeader } from 'src/react/components/BSTHeader/BSTHeader';
import { SortFilter, DefaultSortFilter } from 'src/models/sortfilter';
import { FilterZone } from 'src/react/components/FilterZone/FIlterZone';
import { FindBuySellTradeThread } from 'src/services/FindThread';
import { ParseQueryString, UpdateURL } from 'src/services/WindowServices';
import { SelectType } from 'src/models/selectTypes';

// Auto Fetch
let fetchTimerId: number | null; 
function StartRefresh(Home: Home){
  const f = async () => {
    console.log("Auto Fetching Thread");
    const thread: IBSTThread | IBSTError = await Load();
    if("Code" in thread) {
      // Error
      Home.setState({Thread: null, Companies: [], IsError: true, IsLoading: false, ErrorMessage: makeForError(thread)});
      StopRefresh();
    } else {
      Home.UpdateLoadedThread(thread);
    }
  }
  fetchTimerId = window.setInterval(f, 1000 * 60); // fetch once a minute
}

function StopRefresh(){
  console.log("Stopping Auto Thread Fetch");
  if(fetchTimerId !== null) {
    clearInterval(fetchTimerId);
    fetchTimerId = null;
  }
}

async function CommonCompanies(){
  let companies: Array<Company> = [];
  try {
    companies = await GetCommonCompanies();
  } catch (e) {
    console.log("An error ocurred while fetching common companies:");
    console.log(e);
  }
  return companies;
}

async function Load(): Promise<IBSTThread | IBSTError> {
  try {
    const companies: Array<Company> = await GetCommonCompanies();
    const forumstring: string | IBSTError = await FindBuySellTradeThread();
    if(typeof(forumstring) !== "string") {
      return forumstring;
    }
    const boards: Array<Board> = await GetCommonBoards();
    if(typeof(forumstring) !== typeof("")) {
      return DefautThread;
    }
    const bstRaw : IRedditData | IBSTError = await GetBuySellTradeThreadData(forumstring as string); 
    if("Code" in bstRaw) {
      return bstRaw;
    }
    var y = ParseData(bstRaw, companies, boards);
    return y;
  } catch (e) {
    return DefautThread;
  }
}

function makeSortFilter(m: Map<string, string>, companies: Array<Company>): SortFilter {
 const sf: SortFilter = DefaultSortFilter;
 // Checking Order By
 if(m.has("order")) {
   const v: string = m.get("order")!;
   if(v === "up" || v === "down") {
     sf.Order = v;
   }
 }

 if(m.has("field")) {
   const v: string = m.get("field")!;
   if(
      v === "date_posted" || v === "price" ||
      v === "company" || v === "product" ||
      v === "reply_count" || v === "seller"
    ){
      sf.Field = v;
    }
  } 

 if(m.has("company")) {
   let v: string = m.get("company")!; //comma separated
   sf.Company = [];
   const compArr: string[] = [];
   const companySplits: string[] = v.split(',');
   for(let i = 0; i < companySplits.length; i++) {
      let company: string = companySplits[i].toLocaleLowerCase();
      if( 
        (company === "any" || company === "diy (amateur)" || company === "diy%20(amateur)") 
        && !(company in compArr)) {
          compArr.push(company);
      } 
      else {
        var mm = companies.map(x => x.company.toLocaleLowerCase());     
        if(mm.indexOf(company) >= 0 && !(company in compArr)){
          compArr.push(company);
        }
      }
   }
   sf.Company = compArr;
  }
 if(m.has("bst")) {
   const v: string = m.get("bst")!;
   if( v === "bst" || v === "sell" || v === "buy" || v === "trade") {
     sf.BST = v;
   } 
 }
 return sf;
}

function makeForError(err: IBSTError): string {
  let s: string = "";
  switch(err.Code) {
    case 403:
      // Network error - probably CORS
      s = "We tried to load data from reddit, but the request wasn't able to complete.<br/><br/> " +
      "If you are certain that your internet is working, then you may have Tracking Protection enabled.<br/><br/>" +
      "Some browsers consider cross-domain requests to be tracking, and disable it.<br/><br/>" +
      'Please see the <a class="inlineLink" href="/about">about</a> page for more details.'
      break;
    case 404:
      // file not found
      s = "Couldn't find the Buy Sell Trade thread for this month. Does one exist? If so, please keep it named something close to 'BUY/SELL/TRADE Monthly'<br/><br/>" +
      "If the title format of the thread has changed, please contact the developer on GitHub."
      break;
    default:
      s = "Some error ocurred and we weren't able to load the Buy Sell Trade thread. Sorry.";
      break;
  }
  return "<p>"+s+"</p>";
}

// let LoadedCompanies: Array<Company> = [];
let LoadedThread: IBSTThread = DefautThread;

interface AppState {
  OriginalThread: IBSTThread | null;
  Thread: IBSTThread | null;
  Companies: Array<Company>;
  IsError: boolean;
  IsLoading: boolean;
  ErrorMessage?: string;
  SortFilter: SortFilter;
}

interface AppProps {
  location?: any; // Query string passed by React-Router
}

class Home extends React.Component<AppProps, AppState> {

  constructor(props: AppProps, state: AppState) {
    super(props, state);
    this.state = { 
      Companies: [],
      Thread: DefautThread,
      OriginalThread: DefautThread,
      IsLoading: true,
      IsError: false,
      SortFilter: DefaultSortFilter,
    };
  }

  async componentDidMount() {
    try {
      const thread: IBSTThread | IBSTError = await Load();
      if("Code" in thread) {
        // Error
        this.setState({Thread: null, Companies: [], IsError: true, IsLoading: false, ErrorMessage: makeForError(thread)});
        StopRefresh();
      } else {
        const companies: Array<Company> = AugmentCompanies(await CommonCompanies());
        StartRefresh(this);
        LoadedThread = {
          ...thread,
        };
        const sf: SortFilter = makeSortFilter(ParseQueryString(this.props.location.search), companies)
        this.setState({Thread:this.filter(sf), Companies: companies, IsLoading: false, IsError: false}); // TODO testing IsLoading
      }
    } catch (e) {
      this.setState({Thread: null, Companies: [], IsError: true, IsLoading: false, ErrorMessage: "Something happened"+e});
      StopRefresh();
    }
  }

  componentWillUnmount(){
    StopRefresh();
  }

  public UpdateLoadedThread(t: IBSTThread) : void {
    if(t === LoadedThread) {
      return;
    }
    LoadedThread = t;
    // const sf: SortFilter = makeSortFilter(ParseQueryString(this.props.location.search), this.state.Companies)
    this.setState({Thread:this.filter(this.state.SortFilter), IsLoading: false, IsError: false}); 
  } 


  private filter(sortFilter: SortFilter): IBSTThread {
    let newThread: IBSTThread = {
      Metadata: LoadedThread.Metadata,
      BSTs: [],
    };

    function filter(value: IBSTThreadComment): boolean {
      // checking company
      // console.log(sortFilter);
      if((sortFilter.Company.indexOf("any") == -1)) { 

        if( 
          sortFilter.Company.indexOf(value.Company.toLocaleLowerCase()) == -1 
          && (sortFilter.Company.indexOf("other / unknown") != -1 && value.Company !== '?')
        ) {
          return false;
        }
        // else if(sortFilter.Company.indexOf("other / unknown") != -1) {
        //   if(value.Company !== "?") {
        //     // Item failed match the unknown/orginal special category
        //     return false;
        //   }
        // }  
      }

      // checking BST
      const vbst: string = value.BST.toLocaleLowerCase();
      if(sortFilter.BST === "buy" && vbst !== "buy") {
        return false;
      }
      if(sortFilter.BST === "sell" && vbst !== "sell") {
        return false;
      }
      if(sortFilter.BST === "trade" && vbst !== "trade") {
        return false;
      }
      if(sortFilter.BST === "bst" && (vbst !== "buy" && vbst !== "trade" && vbst !== "sell")) {
        return false;
      }
      return true;
    }

    function sort(a: IBSTThreadComment, b: IBSTThreadComment): number {

      let first: IBSTThreadComment = a;
      let second: IBSTThreadComment = b;
      if(sortFilter.Order === "down") {
        first = b;
        second = a;
      }

      function sortDatePosted(): number { 
        return Number(first.DatePosted) - Number(second.DatePosted);
      }

      function sortPrice(): number {
        return first.Price - second.Price;
      }

      function sortCompany(): number {
        const fname: string = first.Company === "?" ? "" : first.Company.toLocaleLowerCase();
        const sname: string = second.Company === "?" ? "" : second.Company.toLocaleLowerCase();
        if(fname < sname) {
          return -1;
        } else if(fname > sname) {
          return 1;
        } else {
          return 0;
        }
      }

      function sortProduct(): number {
        const fname: string = first.Product === "?" ? "" : first.Product.toLocaleLowerCase();
        const sname: string = second.Product === "?" ? "" : second.Product.toLocaleLowerCase();
        if(fname < sname) {
          return -1;
        } else if(fname > sname) {
          return 1;
        } else {
          return 0;
        }      }

      function sortReplyCount(): number {
        return first.NumberOfReplies - second.NumberOfReplies;
      }

      function sortSeller(): number {
        const fname: string = first.Seller.toLocaleLowerCase();
        const sname: string = second.Seller.toLocaleLowerCase();
        if(fname < sname) {
          return -1;
        } else if(fname > sname) {
          return 1;
        } else {
          return 0;
        }      
      }

      if(sortFilter.Field === "date_posted") {
        return sortDatePosted();
      } else if (sortFilter.Field === "price") {
        return sortPrice();
      } else if (sortFilter.Field === "reply_count") {
        return sortReplyCount();
      } else if (sortFilter.Field === "company") {
        return sortCompany();
      } else if (sortFilter.Field === "seller") {
        return sortSeller();
      } else if (sortFilter.Field === "product") {
        return sortProduct();
      }
      else {
        return 0;
      }
    }

    newThread.BSTs = LoadedThread.BSTs.filter(val => filter(val)).sort(sort);

    return newThread;
    
  }
  private FilterChanged(sortFilter: SortFilter) {
    this.setState({Thread: this.filter(sortFilter), SortFilter: sortFilter});
  }

  private OnCompanyChange(val: SelectType[]) {
    console.log(val);
    const newCompanies: string[] = [];
    for(let i = 0; i < val.length; i++) {
      const st:SelectType = val[i];
      const value = st.value.toLocaleLowerCase();
      newCompanies.push(value);
    }
    if(this.state.Thread && this.state.OriginalThread) { 
      const newSortFilter: SortFilter = {
        ...this.state.SortFilter,
        Company: newCompanies,
    };      
    UpdateURL("company", newCompanies);
    this.FilterChanged(newSortFilter);
  }
}

  private OnOrderByChange(val: string) {
    val = val.toLocaleLowerCase();
    if(this.state.Thread && this.state.OriginalThread) { 
      const newSortFilter: SortFilter = {
        ...this.state.SortFilter,
        Order: val,
      };      
      UpdateURL("order", [val]);
      this.FilterChanged(newSortFilter);
    }
  }

  private OnSortByFieldChange(val: string) {
    val = val.toLocaleLowerCase();
    if(this.state.Thread && this.state.OriginalThread) { 
      const newSortFilter: SortFilter = {
        ...this.state.SortFilter,
        Field: val,
      };    
      UpdateURL("field", [val]);  
      this.FilterChanged(newSortFilter);
    }
  }

  private OnBSTChange(val: string) {
    val = val.toLocaleLowerCase();
    if(this.state.Thread && this.state.OriginalThread) { 
      const newSortFilter: SortFilter = {
        ...this.state.SortFilter,
        BST: val,
      };      
      UpdateURL("bst", [val]);
      this.FilterChanged(newSortFilter);
    }
  }

  private ResetFilters() {
    const newSortFilter: SortFilter = {
      BST: "bst",
      Company: ["any"],
      Field: "date_posted",
      Order: "down"
    };
    UpdateURL(null, null);
    this.FilterChanged(newSortFilter);
  }

  public render() {

    const OnCompanyChange = this.OnCompanyChange.bind(this);
    const OnOrderByChange = this.OnOrderByChange.bind(this);
    const OnSortByFieldChange = this.OnSortByFieldChange.bind(this);
    const OnBSTChange = this.OnBSTChange.bind(this);
    const ResetFilters = this.ResetFilters.bind(this);

    function renderLoading() {
      return (
        <div className="App App-body">
          <BSTHeader/>
          <FilterZone 
            OnCompanyChange={OnCompanyChange} 
            OnOrderByChange={OnOrderByChange} 
            OnSortByFieldChange={OnSortByFieldChange}
            OnBSTChange={OnBSTChange}
            LoadedThread={LoadedThread}
            Sorter={DefaultSortFilter}
            Companies={[]}
            ResetFilters={ResetFilters}
            />

          <div className="BSTZone">
            ...
          </div>

          <BSTFooter />

        </div>
      );
    }

    function renderError(err: string){
      return (
            <div className="App App-body">
            <BSTHeader error={true}/>
            <FilterZone 
              OnCompanyChange={OnCompanyChange} 
              OnOrderByChange={OnOrderByChange} 
              OnSortByFieldChange={OnSortByFieldChange}
              OnBSTChange={OnBSTChange}
              LoadedThread={LoadedThread}
              Sorter={DefaultSortFilter}
              Companies={[]}
              ResetFilters={ResetFilters}
              />
  
            <div className="BSTZone" dangerouslySetInnerHTML={{__html: err}}>
            </div>
  
            <BSTFooter />
  
          </div>
      );
    }

    function renderOk(thread: IBSTThread, companies: Array<Company>, sorter: SortFilter) {
      return (
        <div className="App App-body">

          <BSTHeader thread={thread}/>

          <FilterZone 
            OnCompanyChange={OnCompanyChange} 
            OnOrderByChange={OnOrderByChange} 
            OnSortByFieldChange={OnSortByFieldChange}
            OnBSTChange={OnBSTChange}
            LoadedThread={LoadedThread}
            Thread={thread}
            Sorter={sorter}
            Companies={companies}
            ResetFilters={ResetFilters}
            />
  

          <div className="BSTZone">
            {
              thread.BSTs.map((v: IBSTThreadComment) => {
                const idkey: string = "bstComment_"+v.Seller+"_"+Number(v.DatePosted);
                return (
                  <BSTComment key={idkey} comment={v}/>
                );
              })
            }
          </div>

          <BSTFooter />
        </div>      
      );
    }

    if(this.state.IsLoading) {
      return renderLoading();
    } else if (this.state.IsError) {
      StopRefresh();
      return renderError(this.state.ErrorMessage!);
    } else {
      return renderOk(this.state.Thread!, this.state.Companies, this.state.SortFilter);
    }   
  }
}
export { Home }
