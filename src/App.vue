<template>
  <div id="app">
    <div v-if="!step">
      <button type="" @click="run">run</button>
    </div>
    <div v-else>
      <div>当前步骤：{{ step }}</div>
      <div class="messages">
        <div v-for="({ msg, cls }, i) in messages" :key="i" :class="cls">{{ msg }}</div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import parser from 'wtf_wikipedia';
import chunk from 'lodash/chunk';
import Zip from 'jszip';
import { DictEntry, formatOpf, formatDict } from '@/models/dict';
import { createSite, Site } from '@/models/site';

function saveAs(blob: Blob, name: string) {
  const a = document.createElement('a');

  a.download = name;
  a.rel = 'noopener'; // tabnabbing
  a.href = URL.createObjectURL(blob);
  // todo revokeObjectURL
  setTimeout(function() {
    a.click();
  }, 0);
}

export default Vue.extend({
  name: 'App',
  data() {
    return {
      url: 'https://princeofnothing.fandom.com/wiki/Prince_of_Nothing_Wiki',
      chunkSize: 10,
      step: '',
      messages: [] as any[],
    };
  },
  methods: {
    test() {
      // bot
      //   .fetch('https://princeofnothing.fandom.com/wiki/Anas%C3%BBrimbor_Kellhus')
      //   .then(doc => console.log(doc?.paragraphs(0).text()));
      // bot
      //   .fetch('http://coppermind.huijiwiki.com/wiki/%E6%99%A8%E7%91%9B', { origin: '*' })
      //   .then(doc => console.log(doc?.paragraphs(0).text()));
      // fetch(
      //   'https://coppermind.huijiwiki.com/api.php?action=query&prop=revisions%7Cpageprops&rvprop=content&maxlag=5&rvslots=main&format=json&redirects=true&titles=%E6%99%A8%E7%91%9B',
      //   {
      //     method: 'GET',
      //     headers: {
      //       Origin: '*',
      //     },
      //   },
      // );
      // fetch(
      //   'https://princeofnothing.fandom.com/api.php?action=query&prop=revisions%7Cpageprops&rvprop=content&maxlag=5&rvslots=main&format=json&redirects=true&titles=Anas%C3%BBrimbor_Kellhus&origin=*',
      //   {
      //     method: 'GET',
      //     // headers: {
      //     //   Origin: '*',
      //     // },
      //   },
      // );
      const site = createSite('https://princeofnothing.fandom.com/wiki/Prince_of_Nothing_Wiki');
      this.testSite(site);
      const site2 = createSite('https://coppermind.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5');
      // this.testSite(site2);
    },
    async testSite(site: Site) {
      // site.getDescription().then(console.log);
      // site.getAllPages().then(console.log);
      // site.getAllRedirects().then(console.log);
      const pages = await site.getAllPages();
      const contents = await site.getPageContent(pages.slice(0, 5).map(p => p.title));
      Object.entries(contents).map(([key, content]) => {
        const text = parser(content)
          .sections(0)
          .text();
        console.log(key, text);
      });
    },
    async run() {
      try {
        const site = createSite(this.url);
        const dict: Record<string, DictEntry> = {};
        let progress = 0;
        this.step = '加载基础信息';
        const siteInfo = await site.getDescription();
        this.addMessage(`站点名称：${siteInfo.name}`);
        this.step = '加载词条列表';
        const pages = await site.getAllPages();
        this.addMessage(`共${pages.length}词条`);
        for (const group of chunk(pages, this.chunkSize)) {
          const contents = await site.getPageContent(group.map(p => p.title));
          for (const key of Object.keys(contents)) {
            const section = parser(contents[key]).sections(0);
            if (!section) {
              this.addMessage(`${key}词条获取摘要失败。查看：${siteInfo.url}/wiki/${key}`, 'warn');
              continue;
            }
            dict[key] = { key, description: section.text() };
          }
          progress += group.length;
          this.addMessage(`已下载${progress}/${pages.length}个词条`);
        }
        this.step = '生成OPF文件';
        const opf = formatOpf(siteInfo);
        this.step = '生成HTML文件';
        const html = formatDict(siteInfo, Object.values(dict));
        this.step = '打包';
        const zip = new Zip();
        zip.file(`${siteInfo.name}_dict.opf`, opf);
        zip.file(`kindle_dict.html`, html);
        await zip.generateAsync({ type: 'uint8array' }).then(data => {
          saveAs(new Blob([data], { type: 'application/zip' }), `${siteInfo.name}_dict.zip`);
          this.step = '下载应该已经开始';
        });
      } catch (e) {
        console.error(e);
        this.step = '失败';
        this.addMessage(e.message, 'error');
      }
    },
    addMessage(msg: string, cls = '') {
      this.messages.unshift({ msg, cls });
    },
  },
});
</script>

<style lang="scss">
.messages {
  margin: 10px;
  border: 1px solid #ccc;
  overflow: auto;
  height: 600px;
  .warn {
    background-color: #e6a23c;
  }
  .error {
    background-color: #f56c6c;
  }
}
</style>
