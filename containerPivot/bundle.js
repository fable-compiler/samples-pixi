!function(n){function t(e){if(r[e])return r[e].exports;var o=r[e]={i:e,l:!1,exports:{}};return n[e].call(o.exports,o,o.exports,t),o.l=!0,o.exports}var r={};t.m=n,t.c=r,t.d=function(n,r,e){t.o(n,r)||Object.defineProperty(n,r,{configurable:!1,enumerable:!0,get:e})},t.n=function(n){var r=n&&n.__esModule?function(){return n.default}:function(){return n};return t.d(r,"a",r),r},t.o=function(n,t){return Object.prototype.hasOwnProperty.call(n,t)},t.p="/",t(t.s=130)}({130:function(n,t,r){"use strict";Object.defineProperty(t,"__esModule",{value:!0});var e=r(131);r.d(t,"options",function(){return e.c}),r.d(t,"app",function(){return e.a}),r.d(t,"container",function(){return e.b}),r.d(t,"texture",function(){return e.f}),r.d(t,"renderer",function(){return e.e}),r.d(t,"pivot",function(){return e.d})},131:function(n,t,r){"use strict";r.d(t,"c",function(){return o}),r.d(t,"a",function(){return u}),r.d(t,"b",function(){return i}),r.d(t,"f",function(){return c}),r.d(t,"e",function(){return f}),r.d(t,"d",function(){return p});var e=r(16),o=(r.n(e),{backgroundColor:0}),u=new e.Application(400,400,o);document.body.appendChild(u.view);var i=new e.Container;u.stage.addChild(i);for(var c=e.Texture.fromImage("../img/fable_logo_small.png"),d=0;d<=24;d++){var a=new e.Sprite(c);a.anchor.set(.5),a.x=d%5*40,a.y=40*Math.floor(d/5),i.addChild(a)}var f=u.renderer;i.x=f.width/2,i.y=f.height/2;var p=i.pivot;p.x=i.width/2,p.y=i.height/2,u.ticker.add(function(n){i.rotation=i.rotation-.01*n})},16:function(n,t){n.exports=PIXI}});
//# sourceMappingURL=bundle.js.map