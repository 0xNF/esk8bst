import { Company } from "src/models/company";

/**
 * Given a currency enumeration, return 
 * the appropriate currency character for it.
 * i.e., usd -> $  
 * eur -> £
 * @param currency 
 */
function CurrencyToSymbol(currency: string) {
    switch(currency.toLocaleLowerCase()){
        case "aud":
            return "$AUD";
        case "cad":
            return "$CAD";
        case "gbp":
            return "£";
        case "eur":
            return "€";
        default:
            return "$";
    }
}

/**
 * Used to provide an augmented list of comapnies for the HTML display for filter purposes
 * @param companies 
 */
function AugmentCompanies(companies: Array<Company>): Array<Company> {
    const AllCompany: Company = {
        company: "Any",
        imageUrl: ""
    };
    const UnknownCompany: Company = {
        company: "Other / Unknown",
        imageUrl: "",
    }
    const DIYGeneric : Company = {
        company: "DIY (amateur)",
        imageUrl: "",
    }
    const arr: Array<Company> = [];
    arr.push(AllCompany),
    arr.push(DIYGeneric);
    arr.push(UnknownCompany);
    for(let i = 0; i < companies.length; i++){
        arr.push(companies[i]);
    }
    return arr;
}

function StripMarkdown(md: string): string {
    const s = md
      // Remove HTML tags
      .replace(/<[^>]*>/g, '')
      // Remove setext-style headers
      .replace(/^[=\-]{2,}\s*$/g, '')
      // Remove footnotes?
      .replace(/\[\^.+?\](\: .*?$)?/g, '')
      .replace(/\s{0,2}\[.*?\]: .*?$/g, '')
      // Remove images
      .replace(/\!\[.*?\][\[\(](.*?)[\]\)]/g, '')
      // Remove inline links
      .replace(/\[.*?\][\[\(](.*?)[\]\)]/g, '<a class="inlineLink" target="_blank" href="$1">$1</a>')
      // Remove blockquotes
      .replace(/^\s{0,3}>\s?/g, '')
      // Remove reference-style links?
      .replace(/^\s{1,2}\[(.*?)\]: (\S+)( ".*?")?\s*$/g, '')
      // Remove atx-style headers
      .replace(/^(\n)?\s{0,}#{1,6}\s+| {0,}(\n)?\s{0,}#{0,} {0,}(\n)?\s{0,}$/gm, '$1$2$3')
      // Remove emphasis (repeat the line to remove double emphasis)
      .replace(/([\*_]{1,3})(\S.*?\S{0,1})\1/g, '$2')
      .replace(/([\*_]{1,3})(\S.*?\S{0,1})\1/g, '$2')
      // Remove code blocks
      .replace(/(`{3,})(.*?)\1/gm, '$2')
      // Remove inline code
      .replace(/`(.+?)`/g, '$1')
      // Replace two or more newlines with exactly two? Not entirely sure this belongs here...
      .replace(/\n{2,}/g, '\n\n');    
    return s;
}



/**
 * Sometimes encoded strings like & gt; appear in html blocks
 * This is a cheap function to make them readable as intended
 * @param input html encoded string to decode
 */
function HtmlDecoder(input: string) {
    var e = document.createElement('div');
    e.innerHTML = input;
    return e.childNodes[0].nodeValue;
}

export { CurrencyToSymbol, HtmlDecoder, AugmentCompanies,StripMarkdown }