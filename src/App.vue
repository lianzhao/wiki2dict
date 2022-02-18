<template>
  <div id="app">
    <div class="form">
      <div>维基地址（必填）</div>
      <div>
        <input v-model="url" type="text" :disabled="running" />
      </div>
      <div>语言链接（可选，例如如果是中文维基，则可以输入en以尝试建立中英文之间的关系）</div>
      <div>
        <input v-model="langlink" type="text" :disabled="running" />
      </div>
      <div>是否加载一张图片（注意：这个功能在web端无法使用，请使用nodejs版）</div>
      <div>
        <input v-model="downloadImage" type="checkbox" disabled />
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
      url: '',
      langlink: '',
      downloadImage: false,
      messages: [] as Message[],
      running: false,
    };
  },
  methods: {
    async run() {
      if (!this.url) {
        alert('请输入维基地址');
        return;
      }
      this.running = true;
      try {
        const { siteInfo, data } = await run(this.url, {
          langlink: this.langlink,
          downloadImage: this.downloadImage,
          onMessage: e => this.messages.unshift(e),
        });
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
  input[type='text'] {
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
