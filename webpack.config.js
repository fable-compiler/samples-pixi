var fs = require("fs");
var path = require("path");
var webpack = require("webpack");
var fableUtils = require("fable-utils");
var json5 = require("json5");

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

function getSamples() {
  var samples = {};
  var samplesInfo = json5.parse(fs.readFileSync(resolve("public/samples.json5")));
  for (var currentInfo in samplesInfo) {
    const currentSample = samplesInfo[currentInfo];
    
    // We force fsproj file for each entry to be folderName.fsproj 
    currentSample.entry = currentInfo + ".fsproj";
    const projectFile = path.join(__dirname, "src", currentInfo, currentSample.entry);
    // We include core-js first as it's a polyfill used to support older browsers
    samples[currentInfo] = ["core-js"].concat(currentSample.dependencies, projectFile);
  }
  return samples;
}

var babelOptions = fableUtils.resolveBabelOptions({
  "presets": [
    ["env", {
      "targets": {
        "browsers": ["last 2 versions", "safari >= 7"]
      },
      "modules": false
    }]
  ]
});

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

module.exports = {
  devtool: isProduction ? undefined : "source-map",
  entry: getSamples(),
  output: {
    filename: "[name]/bundle.js",
    path: resolve('public'),
    publicPath: '/'
  },
  resolve: {
    modules: [
      "node_modules", resolve("node_modules")
    ]
  },
  node: {
    fs: 'empty'
  },
  externals: {
    "PIXI": "PIXI",
    "PIXI.extras": "PIXI.extras",
    "PIXI.loaders": "PIXI.loaders",
    "PIXI.settings": "PIXI.settings",
    "PIXI.filters": "PIXI.filters",
    "PIXI.interaction": "PIXI.interaction",
    "PIXI.mesh": "PIXI.mesh",
    "PIXI.particles": "PIXI.particles",
    "PIXI.sound": "PIXI.sound"
  },
  devServer: {
    contentBase: resolve('public'),
    port: 8080
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      },
      {
        test: /\.sass$/,
        use: [
          "style-loader",
          "css-loader",
          "sass-loader"
        ]
      }      
    ]
  }
};
