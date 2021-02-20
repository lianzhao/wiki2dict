import { SiteDescription } from './site';

export interface DictEntry {
  key: string;
  alternativeKeys?: string[];
  description: string;
}

const opfTemplate = `
<?xml version="1.0" encoding="utf-8"?>
<package unique-identifier="uid" version="2.0" xmlns="http://www.idpf.org/2007/opf">
	<!-- 字典元数据 -->
	<metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf">
		<dc-metadata xmlns:dc="http://purl.org/metadata/dublin_core" xmlns:oebpackage="http://openebook.org/namespaces/oeb-package/1.0/">
			<dc:Title>@wikiName字典</dc:Title><!-- 字典名称 -->
			<dc:Language>zh-cn</dc:Language><!-- 字典语言 -->
			<dc:Identifier id="uid">02FFA518EB</dc:Identifier><!-- 字典标识符 -->
			<dc:creator>@wikiName</dc:creator><!-- 创建人 -->
			<dc:publisher>@wikiName</dc:publisher><!-- 发布商 -->
			<dc:date opf:event="publication">@date</dc:date><!-- 发布日期 -->
		</dc-metadata>
		<x-metadata>
			<output encoding="utf-8"/>
			<DictionaryInLanguage>en</DictionaryInLanguage><!-- 输入语言 -->
			<DictionaryOutLanguage>zh-cn</DictionaryOutLanguage><!-- 输出语言 -->
			<EmbeddedCover></EmbeddedCover><!-- 封面图片 -->
		</x-metadata>
	</metadata>
	<!-- 资源列表 -->
	<manifest>
		<item href="kindle_dict.html" id="item1" media-type="text/x-oeb1-document"/><!-- HTML 文件 -->
	</manifest>
	<!-- 骨架文件 -->
	<spine>
		<itemref idref="item1"/> <!-- 对应 <manifest> 中的 html 文件 -->
	</spine>
	<tours/>
	<!-- 页面指引 -->
	<guide>
		<reference href="kindle_dict.html#filepos1" title="Start Reading" type="start"/><!-- 打开起始位置，对应 HTML 文件中的相应 ID -->
	</guide>
</package>
`;
const entryTemplate = `
<idx:entry scriptable="yes">
  <idx:orth value="@word">
    <idx:infl>
      @infl
    </idx:infl>
  </idx:orth>
  <b><word>@word</word></b>
  <br />
  <phonetic></phonetic>
  <category>
    <cat></cat>
    <br />
    <sense>
      <b></b>
      <description>@description</description>
    </sense>
    <br />
  </category>
</idx:entry>
`;
const dictTemplate = `
<html>
  <head>
    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
  </head>
  <body>
    <mbp:pagebreak />
    <p>@wikiName字典</p>
    <br />
    <p><a href="@wikiUrl">版权信息</a></p>

    <mbp:pagebreak />

    <a id="filepos1" />

    <mbp:frameset>
      <hr />
      @entries
      <hr />
    </mbp:frameset>
  </body>
</html>
`;
const pageBreak = '<mbp:pagebreak/>';

export function formatOpf(site: SiteDescription, date?: Date) {
  date = date ?? new Date();
  return opfTemplate.replaceAll('@wikiName', site.name).replaceAll('@date', date.toDateString());
}

export function formatEntry(entry: DictEntry) {
  // todo infl;
  return entryTemplate.replaceAll('@word', entry.key).replaceAll('@description', entry.description);
}

export function formatDict(site: SiteDescription, entries: DictEntry[]) {
  return dictTemplate
    .replaceAll('@wikiName', site.name)
    .replaceAll('@wikiUrl', site.url)
    .replaceAll('@entries', entries.map(formatEntry).join(pageBreak));
}
