import * as React from 'react';
import './NotifyModal.css';
import * as Modal from 'react-modal';
import Select from 'react-select';
import { Company } from 'src/models/company';
import { SelectType } from 'src/models/selectTypes';

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
        };
    }

    
    
    private updatePrice(evt: any) {
        const num: number | undefined = evt.target.value;
        if(num){
            this.setState({price: num});
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

    private checkInputs(){

        if(this.state.email === "" || this.state.email.indexOf("@") == -1 || this.state.email.length < 3) {
            // TODO email is invalid, raise flag
        } else if(this.state.price < 0) {
            // TODO price is negative (but zero is ok), raise flag
        } else {
            // everything's good, let's send a thing to Firebase
            // TODO send to Firebase
        }
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
                    <p>
                        Get notified of when new boards that match your criteria are posted.<br/>
                        We'll send you an email whenever something new comes up. <br/>
                        If you already submitted your email, you can unsubscribe below, or you can use the link in the email.
                    </p>
                    <form>
                        <label htmlFor="fromCompany">From Company</label>
                        <Select id="fromCompany" isMulti={true} options={this.state.companyOptions} onChange={ e => this.OnCompanyChange(e as SelectType[])}  value={this.state.selectedOptions}/>
                        <label htmlFor="maxPrice">Price </label>
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
                        <button onClick={this.checkInputs.bind(this)} type="button">Submit</button>
                        <button onClick={this.props.OnCloseModal} type="button">Cancel</button>
                    </form>
                </div>
            </Modal>
        );
    }
}

export { NotifyModal, NotifyModalProps }