import * as React from 'react';
import './NotifyModal.css';
import * as Modal from 'react-modal';
import Select from 'react-select';
import { Company } from 'src/models/company';
import { SelectType } from 'src/models/selectTypes';
import { Subscriber } from 'src/models/Subscriber';
import { Match } from "src/models/Match";
import { BuySellTrade } from 'src/types/BuySellTrade';
import { SubmitSubscriber, UnsubscribeEmail } from 'src/services/LambdaService';

type NotifyModalDisplay = 
    | "SUBSCRIBE"
    | "UNSUBSCRIBE"
    | "CHECK_EMAIL"
    ;


interface NotifyModalProps {
    IsOpen: boolean;
    OnAfterOpen: () => void;
    OnCloseModal: () => void;
    Companies?: Array<Company>;
}

interface NotifyModalState {
    price: number;
    email: string;
    currency: string;
    companyOptions: SelectType[];
    selectedOptions: SelectType[];
    DisplayState: NotifyModalDisplay;
    bst: BuySellTrade
}

const customStyles = {
    content : {
      top                   : '50%',
      left                  : '50%',
      right                 : 'auto',
      bottom                : 'auto',
      marginRight           : '-50%',
      transform             : 'translate(-50%, -50%)'
    }
  };

const anyObj: SelectType = {
    value: "any",
    label: "any"
};

class NotifyModal extends React.Component<NotifyModalProps, NotifyModalState> {

    public constructor(props: NotifyModalProps) {
        super(props);


        let availableOptions: SelectType[] = !props.Companies ? [] : props.Companies.map((x) => {
            const y = {'value': x.company.toLocaleLowerCase(), label: x.company };
            return y;
          }
        );
        let x = [anyObj];
        x = x.concat(availableOptions);
        availableOptions = x;
        
        this.state = {
            price: 0,
            currency: "USD",
            email: "",
            companyOptions: availableOptions,
            selectedOptions: [anyObj],
            bst: "BST",
            DisplayState: "SUBSCRIBE",
        };
    }
   
    
    private updatePrice(evt: any) {
        let num: number | string | undefined = evt.target.value;
        if(typeof(num) == typeof("")) {
            const nf = parseFloat(num as string);
            const ni = parseInt(num as string);
            console.log("***");
            console.log(ni);
            console.log(nf);
            if(nf !== ni) {
                // kill decimals.
                return;
            } else {
                num = ni;
            }
        }
        if(num){
            this.setState({price: num as number});
            console.log(num);
        }
    }

    private updateEmail(evt: any) {
        const str: string | undefined | null = evt.target.value;
        if(str) {
            this.setState({email: str})
        }
    }

    private OnCurrencyChange(val: any) {
        console.log(val);
        if(val) {
            this.setState({currency: val});
        }
    }

    private checkInputsForSubscribe(){
        console.log(this.state);
        if(this.state.email === "" || this.state.email.indexOf("@") == -1 || this.state.email.length < 3) {
            console.log("Failed submit check because email was invalid: " + this.state.email);
            // TODO email is invalid, raise flag
        } else if(this.state.price < 0 || !Number.isInteger(this.state.price)) {
            console.log("checking type of price: (" + typeof(this.state.price) + ")");
            console.log("Failed submit check because price was invalid: " + this.state.price);
            // TODO price is negative (but zero is ok), raise flag
        } else {
            // everything's good, let's send a thing to Lambda
            // TODO send to Lambda
            let match: Match = {
                companies: [],
                bst: this.state.bst.toLocaleUpperCase()
            };
            // add price key for non-empty prices
            if(this.state.price > 0) {
                match.price = this.state.price;
            }
            // Add currency key for non-usd denominations
            if(this.state.currency !== "USD") {
                match.currency = this.state.currency;
            }
            // Add companies for non-any selections
            
            const selectedCompanies: string[] = this.state.selectedOptions.map(x => x.value);
            if(selectedCompanies.indexOf('any') == -1) { // if any is present, skip. otehrwise, add the value.
                match.companies = selectedCompanies;
            }
            
            let sub: Subscriber = {
                email: this.state.email.toLocaleLowerCase(),
                matches: [match],
            };

            console.log(sub);

            SubmitSubscriber(sub);
            this.setState({DisplayState: "CHECK_EMAIL"});
        }
    }

    private checkInputsForUnsubscribe(){
        UnsubscribeEmail(this.state.email);
        this.props.OnCloseModal();     
    }

    private OnCompanyChange(val: SelectType[]) {
        console.log(val);

        const newCompanies: SelectType[] = [];
        // Add all new filters
        for(let i = 0; i < val.length; i++) {
          const st:SelectType = val[i];
          newCompanies.push(st);
        }
        // Remove the 'any' filter, which doesnt make sense in multi-filter.
        if(newCompanies.length > 1 && newCompanies.indexOf(anyObj) !== -1){
          const anyidx: number = newCompanies.indexOf(anyObj);
          newCompanies.splice(anyidx, 1);
        }
        // Unless there are no other filters, in which case it is implicitly any.
        if(newCompanies.length == 0) {
          newCompanies.push(anyObj);
        }         
        this.setState({selectedOptions: newCompanies});
    }

    private OnSubUnSubChanged(val: any) {
        if(val){
            console.log(val.target.value);
            if(val.target.value == "UNSUBSCRIBE" || val.target.value == "SUBSCRIBE" || val.target.value == "CHECK_EMAIL") {
                this.setState({DisplayState: val.target.value});
            }     
        }
    }

    private OnBSTChange(val: BuySellTrade) {
        console.log(val);
        if(val == "BST" || val == "SELL" || val == "TRADE" || val == "BUY") {
            this.setState({bst: val});
        }
    }


    public render() {
        return (
            <Modal 
                isOpen={this.props.IsOpen}
                onAfterOpen={this.props.OnAfterOpen}
                onRequestClose={this.props.OnCloseModal}
                style={customStyles}
                contentLabel="Example Modal"        
            >
                <div id="modalContent">
                    <div>
                        <label htmlFor="subscribe">Subscribe</label>
                        <input id="subscribe" type="radio" name="sunsunsub" value="SUBSCRIBE" defaultChecked={this.state.DisplayState == "SUBSCRIBE"} onChange={this.OnSubUnSubChanged.bind(this)}/>
                        <label htmlFor="unsubscribe">Unsubscribe</label>
                        <input id="unsubscribe" type="radio" name="sunsunsub" value="UNSUBSCRIBE" defaultChecked={this.state.DisplayState == "UNSUBSCRIBE"} onChange={this.OnSubUnSubChanged.bind(this)}/>
                    </div>

                    {
                        this.state.DisplayState == "SUBSCRIBE" ? 
                            <div className="subscribeZone">
                                <p>
                                    Get notified of when new boards that match your criteria are posted.<br/>
                                    We'll send you an email whenever something new comes up. <br/>
                                    If you already submitted your email, you can unsubscribe below, or you can use the link in the email.
                                </p>
                                <form>
                                    <label htmlFor="lookingTo">People looking to...</label>
                                    <div>
                                        <select id="lookingTo" defaultValue="BST" required={true} onChange={(x: any) => this.OnBSTChange(x.target.value)}>
                                            <option value="BST">Buy, Sell, or Trade</option>
                                            <option value="BUY">Buy</option>
                                            <option value="SELL">Sell</option>
                                            <option value="TRADE">Trade</option>
                                        </select>
                                    </div>
                                    <label htmlFor="fromCompany">From Company</label>
                                    <Select id="fromCompany" isMulti={true} options={this.state.companyOptions} onChange={ e => this.OnCompanyChange(e as SelectType[])}  value={this.state.selectedOptions}/>
                                    <label htmlFor="maxPrice">{this.state.bst == "BUY" ? "Maximum " : "Minimum "} Price</label>
                                    <div>
                                        <input id="maxPrice" type="number" placeholder="600" onInputCapture={this.updatePrice.bind(this)}/> 
                                        <span className="validity"></span>
                                    </div>
                                    <label htmlFor="currencyChoice">Currency</label>
                                    <div>
                                        <select id="currencyChoice" defaultValue="USD" required={true} onChange={(x: any) => this.OnCurrencyChange(x.target.value)}>
                                            <option value="USD">USD ($)</option>
                                            <option value="CAD">CAD ($)</option>
                                            <option value="AUD">AUD ($)</option>
                                            <option value="GBP">GBP (£)</option>
                                            <option value="GBP">EUR (€)</option>
                                        </select>
                                    </div>

                                    <label htmlFor="yourEmail">Email</label>
                                    <div>
                                        <input id="yourEmail" required={true} type="email" placeholder="boards@ebay.com" minLength={3} min="3" onInputCapture={this.updateEmail.bind(this)}/> 
                                        <span className="validity"></span>
                                    </div>
                                    <button onClick={this.checkInputsForSubscribe.bind(this)} type="button">Submit</button>
                                    <button onClick={this.props.OnCloseModal} type="button">Cancel</button>
                                </form>
                            </div>
                        :
                        this.state.DisplayState == "UNSUBSCRIBE" ?

                            <div className="unsubscribeZone">
                                <form>
                                    <label htmlFor="yourEmail">Email</label>
                                    <div>
                                        <input id="yourEmail" required={true} type="email" placeholder="boards@ebay.com" minLength={3} min="3" onInputCapture={this.updateEmail.bind(this)}/> 
                                        <span className="validity"></span>
                                    </div>
                                    <button onClick={this.checkInputsForUnsubscribe.bind(this)} type="button">Unsubscribe</button>
                                    <button onClick={this.props.OnCloseModal} type="button">Cancel</button>
                                </form>
                            </div>

                            :

                            <div className="CheckEmailZone">
                                We've sent you an email confirming your address.<br/>
                                Be sure to click on the confirmation link to receive email updates.<br/>

                                <button onClick={this.props.OnCloseModal} type="button">Ok</button>
                            </div>
                    }
                    
                </div>
            </Modal>
        );
    }
}

export { NotifyModal, NotifyModalProps }