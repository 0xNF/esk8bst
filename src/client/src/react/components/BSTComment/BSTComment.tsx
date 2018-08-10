import * as React from 'react';
import { IBSTThreadComment } from 'src/models/IBST';
import { CurrencyToSymbol, HtmlDecoder, StripMarkdown } from 'src/services/DispalyMappers';
import 'src/react/pages/Home/App.css';

interface BSTCommentProps {
    comment: IBSTThreadComment;
}

function BSTComment(props: BSTCommentProps) {
    return (
        <div className="BSTComment">
        <div className="itemInfo flex-grid">
            <div className="col" style={{textAlign: 'left'}}>  
              <span>
                <span className="BSTCommentMeta">Posted At: </span>{props.comment.DatePosted.toLocaleString()}<br/>
                <span className="BSTCommentMeta">Seller: </span>{props.comment.Seller}<br/>
                <span className="BSTCommentMeta">Looking To: </span>{props.comment.BST}<br/>
                <span className="BSTCommentMeta">Price: </span>{CurrencyToSymbol(props.comment.Currency) + props.comment.Price}<br/>
            </span>
          </div>
          <div className="col" style={{textAlign: 'right'}}>       
            <span>
              <span className="BSTCommentMeta">Company: </span>{props.comment.Company}<br/>
              <span className="BSTCommentMeta">Product: </span>{props.comment.Product}<br/>
              <a className="inlineLink" href={props.comment.Url} target="_blank">See Post</a>
            </span>
          </div>
        </div>
        <br/>
        <div style={{textAlign: 'left'}} dangerouslySetInnerHTML={{__html: StripMarkdown(HtmlDecoder(props.comment.Text)!)}}>
          {/* {StripMarkdown(HtmlDecoder(v.Text)!)} */}
        </div>
      </div>
    );
}

export { BSTComment, BSTCommentProps }