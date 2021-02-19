<template>
  <div id="app">
    <button type="" @click="test">test</button>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import bot from 'wtf_wikipedia';

export default Vue.extend({
  name: 'App',
  methods: {
    test() {
      bot
        .fetch('https://princeofnothing.fandom.com/wiki/Anas%C3%BBrimbor_Kellhus')
        .then(doc => console.log(doc?.paragraphs(0).text()));
    },
  },
});
</script>
