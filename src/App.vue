<template>
  <div id="app">
    <button type="" @click="test">test</button>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import bot from 'wtf_wikipedia';
import { createSite, Site } from '@/models/site';

export default Vue.extend({
  name: 'App',
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
      this.testSite(site2);
    },
    testSite(site: Site) {
      site.GetDescription().then(console.log);
    },
  },
});
</script>
