# wiki2dict
Generate dictionary from wiki

# download
* [红铜智库](http://coppermind.huiji.wiki/) kindle dict [download](https://github.com/lianzhao/wiki2dict/raw/master/dist/dict.mobi)

# How to use kindle dict
http://kindlefere.com/dict

# For developers
##How it works
1. Generate a [html file](/dist/dict.html) from wiki data
2. Use [KindleGen](http://www.amazon.com/gp/feature.html?ie=UTF8&docId=1000765211) to generate the [dict file](/dist/dict.mobi), [step by step](http://kindlefere.com/post/178.html)

##For Windows developer
1. Open Wiki2Dict.sln with Visual Studio 2015
2. Run

##For Mac developer
1. Install latest dnx. See [documents](http://docs.asp.net/en/latest/getting-started/installing-on-mac.html)
2. `cd src/Wiki2Dict`
3. `dnx run`