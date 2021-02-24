<template>
  <div id="app">
    <div class="form">
      <div>维基地址</div>
      <div>
        <input v-model="url" :disabled="running" />
      </div>
      <button type="" :disabled="running" @click="run">开始</button>
    </div>
    <div class="messages">
      <div v-for="(msg, i) in messages" :key="i" :class="msg.level">
        {{ msg.message }}
        <a v-if="msg.helpLink" :href="msg.helpLink">帮助</a>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import parser from 'wtf_wikipedia';
import { Site } from '@/models/site';
import run, { Message } from './run';

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
      messages: [] as Message[],
      running: false,
    };
  },
  methods: {
    test() {
      parser.fetch('http://www.tolkiengateway.net/wiki/Main_Page').then(doc => console.log(doc?.paragraphs(0).text()));
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
      // const site = createSite('https://princeofnothing.fandom.com/wiki/Prince_of_Nothing_Wiki');
      // this.testSite(site);
      // const site2 = createSite('https://coppermind.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5');
      // this.testSite(site2);
    },
    async testSite(site: Site) {
      // const pages = await site.getAllPages();
      // const contents = await site.getPageContent(pages.slice(0, 5).map(p => p.title));
      // Object.entries(contents).map(([key, content]) => {
      //   const text = parser(content)
      //     .sections(0)
      //     .text();
      //   console.log(key, text);
      // });
      site.getAllRedirects().then(console.log);
    },
    async run() {
      this.running = true;
      try {
        const { siteInfo, data } = await run(this.url, { onMessage: e => this.messages.unshift(e) });
        saveAs(new Blob([data], { type: 'application/zip' }), `${siteInfo.name}_dict.zip`);
      } catch (e) {
        console.error(e);
        this.messages.unshift({ message: `任务失败：${e.message}`, level: 'error', helpLink: '' });
      } finally {
        this.running = false;
      }
    },
  },
});
</script>

<style lang="scss">
.form {
  input {
    width: 100%;
  }
}
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
