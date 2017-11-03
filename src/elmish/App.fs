module Pixi

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS
open Fable.Pixi
open Elmish
open Elmish.React
open Fulma.Components
open Fulma.Elements
open Fulma.Layouts
open Fable.Helpers.React
open Fable.Helpers.React.Props

// Fulma css
importAll "../../public/sass/main.sass"

[<Literal>]
let randomDragonsToAdd = 10.

// Elmish Model
type Model = { 
  current : int 
}

// ExtendedSprite
type Dragon = {
  mutable angle:float
}

// Render loop States
type State = 
  | Start
  | AddDragon of int
  | Render

// Pixi Model
type Screen =  {
  mutable dragons : ExtendedSprite<Dragon> list 
  mutable state : State 
  mutable model: Model
}

// Elmish Message
type Msg = 
  | AddMoreDragons 

module ElmishApp = 

  // Elmish init 
  let initState model _ =
    model, Cmd.none

  // Elmish update
  let update (screen:Screen) msg (model,_)  =
    match msg with
    | AddMoreDragons  -> 
      // add a random number of dragons
      let count = (Math.random() * randomDragonsToAdd + 1. |> int)
      let newModel = { model with current = model.current + count }
      
      screen.model <- newModel // update reference to Elmish model in Pixi model
      screen.state <- AddDragon count // change the state of the Pixi model so that we can add more dragons
      
      newModel, Cmd.none

  // Elmish view using React and Fulma
  let view (model,_) dispatch = 
  
    let dragonsCounter model = 
      Level.level [ ]
        [ 
          Level.item [ Level.Item.hasTextCentered ] [
            div [ ] [ 
              Level.heading [ ] [ str "Dragons" ]
              Level.title [ Level.customClass "counter" ] [ str (sprintf "%i" model.current) ] 
            ] 
          ]
        ]

    let instructionsBox = 
      Box.box' [  ] [ 
        str "Click on a Dragon or the yellow button to add more dragons!"]

    let addDragonButton dispatch = 
      Button.button_a [ 
        Button.isWarning 
        Button.props [
          OnClick (fun _ -> AddMoreDragons |> dispatch)
        ]
      ] [ 
        str "Add more Dragons!" 
      ] 

    // our view
    div [ ClassName ""][
      Navbar.navbar [ Navbar.isBlack ]
        [ 
          Navbar.brand_div [] [ 
            Navbar.item_div [][ yield dragonsCounter model]
          ]
          Navbar.start_div [] [
            Navbar.item_div [] [ yield instructionsBox]
          ]
          Navbar.end_div [] [ 
            Navbar.item_div [][ yield addDragonButton dispatch ] 
          ] 
        ]
    ]

  let start screen pixiLoop= 
    Program.mkSimple (initState screen.model) (update screen) view
    |> Program.withSubscription pixiLoop // we'll get our messages from the renderer using Elmish subscription system
    |> Program.withReact "elmish-app" // bind our React app to this Html Div element 
    |> Program.run    

module PixiApp = 

  let addDragons root (rwidth,rheight) count dispatch = 
    [
      for i in 0..count-1 do 
        let castTo (s:PIXI.Sprite) = s :?> ExtendedSprite<Dragon>
        let scale = Math.random() + 0.3
        let texture = SpriteUtils.getTexture "dragon"
        let sprite = 
          ExtendedSprite<Dragon>(texture,{angle=0.})
          |> SpriteUtils.setAnchor 0.5 0.5
          |> SpriteUtils.moveTo (rwidth * Math.random()) (rheight * Math.random()) 
          |> SpriteUtils.scaleTo scale scale 
          |> SpriteUtils.setAlpha scale 
          |> SpriteUtils.makeButton
          |> SpriteUtils.addToContainer root
          |> castTo

        sprite.on("pointerdown", fun _ -> 
          (AddMoreDragons |> dispatch) |> ignore
        ) |> ignore
        
        yield sprite
    ]
  
  let updateState root (rwidth,rheight) screen dispatch = 

    match screen.state with 
    | Start -> 
      
      AddDragon 1

    | Render -> // simply rotate our dragons
      
      for dragon in screen.dragons do
        dragon.Data.angle <- dragon.Data.angle + 0.05
        let scale : PIXI.Point = !!dragon.scale
        dragon.rotation <-  dragon.Data.angle  * (1.0 - scale.x)

      Render

    | AddDragon n -> // add new sprites to the rendering

      let newDragons = addDragons root (rwidth,rheight) n dispatch
      screen.dragons <- screen.dragons @ newDragons

      Render
  
  // our render loop  
  let renderLoop root (ticker:PIXI.ticker.Ticker) (rwidth,rheight) screen _ = 

    let sub dispatch = 
      ticker.add (fun _ -> 
        screen.state <- updateState root (rwidth,rheight) screen dispatch      
      ) |> ignore    
    
    Cmd.ofSub sub
    
  // Let's create our pixi application
  let createApp (domElement:HTMLElement) = 

    let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
      o.antialias <- Some true
      o.transparent <- Some true
    )
    let app = PIXI.Application(window.innerWidth, window.innerHeight, options)
    domElement.appendChild(app.view) |> ignore    

    app

// load our assets and stat the app
let loadAssets onLoadComplete = 

  let loader = PIXI.loaders.Loader()
  let path = "../img/"
  [
    ("dragon",sprintf "%s/fable_logo_small.png" path)
  ] 
  |> Seq.iter( fun (name,path) -> loader.add(name,path) |> ignore  )

  loader.load( fun _ (res:PIXI.loaders.Resource) ->
    
    // fill our Asset store 
    Assets.addTexture "dragon" !!res?dragon?texture 

    // our nasty callback ;-)
    onLoadComplete()

  ) |> ignore


let startGame (app:PIXI.Application) =  

  // our root container.
  let root = PIXI.Container() 
  app.stage.addChild root |> ignore

  // our pixi model
  let screen = { dragons=[];state=Start; model={current=1}}

  // prepare ticker for pix render loop
  let ticker = app.ticker
  ticker.stop()

  // let's start our Elmish program
  let renderer : PIXI.WebGLRenderer = !!app.renderer
  let pixiLoop = PixiApp.renderLoop root ticker (renderer.width,renderer.height) screen
  ElmishApp.start screen pixiLoop 

  // let's start our pixi loop
  ticker.start()

// create our pixi app
let domElement : HTMLDivElement = !!document.getElementById("pixi-layout")
let app = PixiApp.createApp domElement

// load our assets and start our app
loadAssets (fun _ -> startGame app)