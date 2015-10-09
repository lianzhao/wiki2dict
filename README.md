# wiki2dict
Generate dictionary from wiki

# How to generate a dictionary for Kindle
1. Customize target wiki and dictionary template files.
2. Generate a [html file](/dist/dict.html) from wiki data
3. Use [KindleGen](http://www.amazon.com/gp/feature.html?ie=UTF8&docId=1000765211) to generate the dict file by yourself. See [Documents](http://kindlefere.com/post/178.html)

## Customize
1. Customize target wiki in [this file](/src/Wiki2Dict/wiki.json)
2. Customize dictionary template files in [resources](/resources)

## Generate that html file
### For Windows developers
1. Open `Wiki2Dict.sln` with Visual Studio 2015
2. Run

### For Mac developers
1. Install latest dnx. See [documents](http://docs.asp.net/en/latest/getting-started/installing-on-mac.html)
2. `cd src/Wiki2Dict`
3. `dnx run`

# License Agreements
* [MIT license](LICENSE)
* The dictionaries generated are `NOT` part of this project. See the license agreements of the individual dictionary for detail.