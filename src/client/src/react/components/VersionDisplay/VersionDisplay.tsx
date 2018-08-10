import * as React from 'react';
import { VersionInfo } from 'src/models/version';
import './VersionDisplay.css';
import { FetchVersion } from 'src/services/LoadVersion';


interface VersionDisplayProps {
   
}

interface VersionDisplayState {
    version?: VersionInfo;
}

class VersionDisplay extends React.Component<VersionDisplayProps, VersionDisplayState> {

    props: VersionDisplayProps;

    constructor(props: VersionDisplayProps) {
        super(props);
        this.props = props;
        this.state = {}
    }
    
    async componentDidMount() {
        try {
            const x = await FetchVersion();
            console.log(x);
            this.setState({version: x});
        } catch (e) {
            console.log("ay wut mate");
        }
    }

    public render() {
        return (
            <span className="verinfo">
                Version {this.state.version ? this.state.version.version : ""}<br/>
                Deployed On: { this.state.version ?  new Date(this.state.version.built).toLocaleString() : ""}
            </span>
        )
    }
}

export { VersionDisplay, VersionDisplayProps }