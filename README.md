![Samples-pixi](public/img/samples-pixi.png)

# Libraries

Bindings & samples for Pixi.js related libraries including:
- Pixi.js
- [Pixi-particles](https://github.com/pixijs/pixi-particles)
- [Pixi-Sound](https://github.com/pixijs/pixi-sound)
- [Animejs tweening library](http://animejs.com/)

### Pixi-particles

![Pixi-Particles](public/img/dragon.gif)

[Pixi-particles](https://github.com/pixijs/pixi-particles)'s really easy to use and there's a great [online editor](https://pixijs.github.io/pixi-particles-editor/) to configure your particle effects easily

>Go to the `src/particles` folder to have a go. And make changes to the json located under `public/img/emitter.json` to see what you can do with particles.

### Pixi-Sound
We also support [Pixi-Sound](https://github.com/pixijs/pixi-sound), the official pixi sound API.

> Go to the `src/sound` folder to see a sample in action.


### Animatejs Tweening

![Animejs](public/img/animejs.gif)

We support [Animejs](http://animejs.com/) for all your tweenings!

> Go to the `src/animejs` folder to see a sample in action.

# Samples

We've got you covered with 19 straight to the point samples which are either ports from the PixiJS official ones or original ones.

## Original samples

### Dragon Particle
![DragonParticle](public/img/dragonParticle.gif)

Learn how to move a particle emitter around som SVG shape like our Fable Dragon!

You'll know how to:
- Add particles using pixi-particles
- play simple animations using AnimeJS and html SVG path


Source code can be found [here](src/dragonParticle)

### Game of Cogs
![Game of cogs](public/img/gameofcogs.gif)

Learn how to make a complete mini game:
- Title screen
- Game Screen
- Loading assets, pictures and json files and sounds, through pixi asset loader
- Playing sounds using pixi-sound
- Adding particles using pixi-particles
- playing simple animations using AnimeJS 

Source code can be found [here](src/draggor)

# How to build and run the samples

- Restore NPM dependencies: `yarn install`
- Restore NuGet dependencies: `dotnet restore`
- **Move to src folder**: `cd src`
- Start Fable and Webpack dev server: `dotnet fable yarn-start`
- In your browser, open `localhost:8080/[EXAMPLE]` (e.g. `http://localhost:8080/basic`) or just select a sample in the list at `http://localhost:8080/`

Any modification you do to the F# code will be reflected in the web page after saving.
If you want to write JS files to disk instead of using the development server,
run `dotnet fable yarn-build`.

# How to add a new sample

- Take one of the existing samples as a reference.
- Add the information about your sample to `public/samples.json5`: id,  title and description
- Add one folder named after the id of the sample to `src` directory and another one to `public`. The first one will contain the F# (and maybe JS) source files, while the second contains the public assets for the sample (like index.html, images, etc).
- Add the project to the `Fable.Samples.sln` solution: `dotnet sln add src/mySample/mySample.fsproj`. **Important: the name of your fsproj file must be the same you use for your folder to allow for automatic build.**(folder `greatProject` -> `greatProject.fsproj`)
- Restore NuGet dependencies: `dotnet restore`

## Webpack configuration

Pixi requires to set additional externals to work with webpack. Like this:

```json
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
```

Would you stumble on errors like this: `Module not found: Error: Can't resolve 'PIXI.xxx' in ...`,  just add the module to the Webpack config.
