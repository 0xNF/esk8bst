import * as React from 'react';
import { IBSTThread } from 'src/models/IBST';
import { SortFilter } from 'src/models/sortfilter';
import { Company } from 'src/models/company';
import Select from 'react-select';
import './FilterZone.css';
import { SelectType } from 'src/models/selectTypes';

interface FilterZoneProps {
    Thread? : IBSTThread;
    Sorter: SortFilter;
    Companies: Array<Company>;
    ResetFilters: () => void;
    OnCompanyChange: (val: SelectType[]) => void;
    OnSortByFieldChange: (val: string) => void;
    OnOrderByChange: (val: string) => void;
    OnBSTChange: (val: string) => void;
    LoadedThread: IBSTThread;
}

function FilterZone(props: FilterZoneProps) {

    const availableOptions = props.Companies.map((x) => {
        const y = {'value': x.company.toLocaleLowerCase(), label: x.company };
        return y;
      }
    );

    const selectedOptions = props.Sorter.Company.map( (x) => {
      const y = {'value': x, label: x };
      return y;
    });

    return (
        <div className="FilterZone">
        <div className="flex-grid filterflex">
          <div className="col colLabel">  
            {/* Sort on Field */}
            <label htmlFor="sortBy">Sort By</label>
          </div>
          <div className="col colSelect">
            <select id="sortBy" name="sortBy" value={props.Sorter.Field} onChange={ e => props.OnSortByFieldChange(e.target.value)}>
                <option value="date_posted">Date Posted</option>
                <option value="price">Price</option>
                <option value="product">Product</option>
                <option value="company">Company</option>
                <option value="seller">Seller</option>
                <option value="reply_count">Reply Count</option>
            </select>
          </div>            
        </div>
        <div className="flex-grid filterFlex">
          <div className="col colLabel">
            <label htmlFor="orderBy">Order By</label>
          </div>
          <div className="col colSelect">  
            {/* Order by */}
            <select id="orderBy" value={props.Sorter.Order} onChange={ e => props.OnOrderByChange(e.target.value)}>
              <option value="down">Descending</option>
              <option value="up">Ascending</option>
            </select>
          </div>  
        </div>
        <div className="flex-grid filterflex">
          <div className="col colLabel">  
            {/* Filter on BTS type */}
            <label htmlFor="filterBTS">Looking to...</label>
          </div>
          <div className="col colSelect">
            <select id="filterBTS" value={props.Sorter.BST} onChange={ e => props.OnBSTChange(e.target.value)}>
              <option value="bst">Buy, Sell, or Trade</option>
              <option value="buy">Buy</option>
              <option value="sell">Sell</option>
              <option value="trade">Trade</option>
            </select>
          </div>        
        </div>
        <div className="flex-grid filterflex">
            <div className="col colLabel">  
              {/* Filter on BTS type */}
              <label htmlFor="filterBTS">From Company</label>
            </div>
            <div className="col colSelect">
              {/* Filter on Company
              <div>
                <label htmlFor="filterCompany">From Company</label>
              </div> */}
                <Select 
                  className="selectText"
                  options={availableOptions}
                  isMulti={true}
                  onChange={ e => props.OnCompanyChange(e as SelectType[])}
                  value={selectedOptions}
                />
            </div>
          </div>
        <div>
          <button onClick={props.ResetFilters}>Reset Sort/Filters</button>
            {
              (props.Thread && (props.Thread.BSTs.length !== props.LoadedThread.BSTs.length)) ? 
                <div>
                  <span>Results: {props.Thread.BSTs.length} / {props.LoadedThread.BSTs.length} </span>
                </div>
                : 
                ""
              }
          </div>
      </div>
    );
}

export { FilterZone, FilterZoneProps }