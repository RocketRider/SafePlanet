//VERSION: 0.2 (2014-04-04)
//Copyright (c) 2015 Stefan Moebius
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
//associated documentation files (the "Software"), to deal in the Software without restriction, 
//including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial
//portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
//LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

SEARCHSTR  = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_#!+?$%~";
REPLACESTR = "28a~$6uJ+Vdjgx01DozBOfs4SvXF_#ArH59MTY%ipqnQRWew!lyZGULm7tCEhKP?3NIcbk";

function encode(text, str_search, str_replace)
{
	newtext = "";
	for (i = 0; i < text.length; i++) {
		ch = text[i];
		for (j=0; j < str_search.length; j++) {
			if (ch === str_search[j]) {
				ch = str_replace[j];
				break;
			}
		}
		newtext += ch;
	}
	return newtext;
}

function onClickHandler(info, tab) {
	if (info.menuItemId === 'enctools_insetpwd') {
		
		jscode = 'str_search = "' + SEARCHSTR + '";';	
		jscode += 'str_replace = "' + REPLACESTR + '";';
				
		jscode += 'document.execCommand("paste");';
				
		jscode += 'text = document.activeElement.value;';			
		jscode += 'if (text.length > 0) {';
		jscode += '  if (text.substr(0, 1) === "µ") {';
		jscode += '    newtext = "";';		
		jscode += '    text = text.substr(1, text.length-1);';
		jscode += '    for (i = 0; i < text.length; i++) {';
		jscode += '      ch = text[i];';
		jscode += '	     for (j = 0; j < str_search.length; j++) {';
		jscode += '		   if (ch === str_search[j]) {';
		jscode += '			 ch = str_replace[j];';
		jscode += '			 break;';
		jscode += '		   }';
		jscode += '      }';		
		jscode += '	     newtext += ch;';
		jscode += '    }';
		jscode += '    text = newtext;';				
		jscode += '  }';
		jscode += '}';
		jscode += 'document.activeElement.value = text;';
		
		//jscode = 'document.activeElement.value = "test";';
		//try {
		chrome.tabs.executeScript({code: jscode});
		//} 
		//catch(e)
		//{
		//	console.log(e.stack);
		//}		
	}
	else if (info.menuItemId === 'enctools_encode') {
		text = prompt("Please enter password to encode", "");
		text = encode(text, REPLACESTR, SEARCHSTR);
			prompt("Encoded password", "µ" + text);		
	}
	else if (info.menuItemId === 'enctools_decode') {
		text = prompt("Please enter password to decode", "");
		if (text.substr(0, 1) === 'µ')
		{
			text = encode(text.substr(1, text.length-1), SEARCHSTR, REPLACESTR);
			prompt("Decoded password", text);
		}
		else
		{
			alert("Cannot decode password!");
		}
	}
};

chrome.contextMenus.onClicked.addListener(onClickHandler);

chrome.runtime.onInstalled.addListener(function() {
    var context = "editable";
    var title = "Insert encoded password";
    var id = chrome.contextMenus.create({"title": title, "contexts":[context], "id": "enctools_insetpwd"});                   
    console.log("added '" + context + "' item:" + id);
    
    context = "page";
    title = "Encode/Decode Tools";
    id = chrome.contextMenus.create({"title": title, "contexts":[context], "id": "enctools"});                                                            
    console.log("added '" + context + "' item:" + id);
    
    title = "Encode a password";
	id = chrome.contextMenus.create(
		{"title": title, "parentId": "enctools", "id": "enctools_encode"});
	console.log("added child '" + context + "' item:" + id);
	
	title = "Decode a password";
	id = chrome.contextMenus.create(
		{"title": title, "parentId": "enctools", "id": "enctools_decode"});
	console.log("added child '" + context + "' item:" + id);
    
});
