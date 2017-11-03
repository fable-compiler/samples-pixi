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

// Elmish init 
let initState model _ =
  model, Cmd.none

// Elmish update
let update (screen:Screen) msg (model,_)  =
  match msg with
  | AddMoreDragons  -> 
    let count = (Math.random() * randomDragonsToAdd + 1. |> int)
    let newModel = { model with current = model.current + count }
    screen.model <- newModel // update reference to Elmish model in Pixi model
    screen.state <- AddDragon count // change the state of the Pixi model so that we can add more dragons
    newModel, Cmd.none

// Elmish view
let view (model,_) dispatch = 
  div [ ClassName ""][
    Navbar.navbar [ Navbar.isBlack ]
      [ 
        Navbar.brand_div [ ]
          [ 
          Navbar.item_div [ ]
            [
              Level.level [ ]
                [ 
                  Level.item [ Level.Item.hasTextCentered ] [
                    div [ ] [ 
                      Level.heading [ ] [ str "Dragons" ]
                      Level.title [ Level.customClass "counter" ] [ str (sprintf "%i" model.current) ] 
                    ] 
                  ]
                ]
            ]            
          ]
        Navbar.start_div [] [
          Navbar.item_div [ ]
            [
              Box.box' [  ] [ 
                str "Click on a Dragon or the yellow button to add more dragons!"]
            ]
        ]
        Navbar.end_div [ ] [ 
          Navbar.item_div [ ]
            [ 
              Button.button_a [ 
                Button.isWarning 
                Button.props [
                  OnClick (fun _ -> AddMoreDragons |> dispatch)
                ]
              ] [ 
                str "Add more Dragons!" 
              ] 
            ] 
          ] 
      ]
  ]

// start our main loop
let init screen container (ticker:PIXI.ticker.Ticker) (rwidth,rheight)= 

  // We start by loading our assets 
  let loader = PIXI.loaders.Loader()
  let path = "../img/"
  [
    // particle confi files
    ("dragon",sprintf "%s/fable_logo_small.png" path)
  ] 
  |> Seq.iter( fun (name,path) -> loader.add(name,path) |> ignore  )

  loader.load( fun _ (res:PIXI.loaders.Resource) ->
    
    // fill our Asset store 
    Assets.addTexture "dragon" !!res?dragon?texture 

    // our render loop  
    let startLoop _ = 

      let sub dispatch = 
        ticker.add (fun _ -> 

          screen.state <- 
            match screen.state with 
            | Start -> 
              AddDragon 1

            | Render -> 
              for dragon in screen.dragons do
                dragon.Data.angle <- dragon.Data.angle + 0.05
                let scale : PIXI.Point = !!dragon.scale
                dragon.rotation <-  dragon.Data.angle  * (1.0 - scale.x)

              Render

            | AddDragon n -> 

              for i in 0..n-1 do 
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
                  |> SpriteUtils.addToContainer container
                  |> castTo

                sprite.on("pointerdown", fun _ -> 
                  (AddMoreDragons |> dispatch) |> ignore
                ) |> ignore

                screen.dragons <- screen.dragons @ [sprite]

              Render
        
        ) |> ignore    
      
      Cmd.ofSub sub

    // let's start our Elmish program
    Program.mkSimple (initState screen.model) (update screen) view
    |> Program.withSubscription startLoop
    |> Program.withReact "elmish-app"
    |> Program.run    

    // let's start our pixi loop
    ticker.start()

  ) |> ignore


let start() = 
  // Let's create our pixi application
  let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
    o.antialias <- Some true
    o.transparent <- Some true
  )
  let app = PIXI.Application(window.innerWidth, window.innerHeight, options)

  // add our app to our div element
  let pixiCanvas : HTMLDivElement = !!document.getElementById("pixi-layout")
  pixiCanvas.appendChild(app.view) |> ignore

  let renderer : PIXI.WebGLRenderer = !!app.renderer

  let container = PIXI.Container() 
  app.stage.addChild container |> ignore

  // our pixi model
  let screen = { dragons=[];state=Start; model={current=1}}

  // our rendering loop mechanism
  let ticker = app.ticker
  ticker.stop()

  init screen container ticker (renderer.width,renderer.height)// it all begins there

// let's start our proof of concept!
start()