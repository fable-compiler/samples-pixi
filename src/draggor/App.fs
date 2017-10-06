module App

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
//open Fable.Import.JS
open Fable.Pixi
open Elmish
open Hink
open Types
open Cog

// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let renderer : PIXI.WebGLRenderer = !!app.renderer

// our layers 
// markers layer
let targetsContainer = PIXI.Container()
app.stage.addChild targetsContainer |> ignore

// found cogs layer
let cogsContainer = PIXI.Container()
app.stage.addChild cogsContainer |> ignore
  
// dragging cogs layer
let dockContainer = PIXI.Container()
app.stage.addChild dockContainer |> ignore
  
let turnCogs model =
  
  model.Found
    |> Seq.iter( fun i ->
      let target = model.Targets.[i] 
      // animate all the cogs that have been found
      // make sure the next cog will turn on the opposite direction
      let way = if i % 2 = 0 then 1. else -1.0
      // smaller cogs run faster
      let speed = Cog.rotation * (1.25 - (Cog.scaleFactor target.Data.Size))
      target.rotation <- target.rotation + way * speed
    )
  
// our render loop
let tick delta =

  model <- 
    match model.State with 
      | Init -> 

        {model with 
          Goal=Cog.cogSizes.Length // the cogs to place correctly in order to win
          State=PlaceCogs}

      | PlaceCogs -> 
        // create our cogs and center them!
        let targets = 

          // simply add our cogs
          // There's really nothing complicate here
          // Each cog is placed according to the previous one
          // hence the recursion 
          let rec addCog index (totalWidth,totalHeight) first previous cogs maxWidth= 
            
            match index with
            | idx when idx < model.Goal -> // check whether we can add one more cogs
              if totalWidth < maxWidth then // check wether we have enough space laeft
                let kind = Cog.cogSizes.[index]
                let currentCog = Cog.make kind
                match previous with 
                | Some (previousCog:ExtendedSprite<CogData>) ->
                  
                  // we want to move our new cog just next to the previous
                  let (firstX,firstY) = first
                  let (prevX,prevY) = previousCog.Data.Target
                  let prevRadius = Cog.cogWidth * (Cog.scaleFactor previousCog.Data.Size) * 0.5
                  let currentRadius = Cog.cogWidth * (Cog.scaleFactor currentCog.Data.Size) * 0.5 * 1.1
                  let distance = (prevRadius + currentRadius) |> JS.Math.floor
                  let angle = 340. * PIXI.Globals.DEG_TO_RAD
                  let newX = prevX + JS.Math.cos(angle) * distance
                  let newY = firstY + JS.Math.sin(angle) * distance
                  let current = currentCog |> Cog.addTarget newX newY     
                  
                  let totalWidth = newX + currentRadius
                  let totalHeight = newY + currentRadius
                  addCog (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current]) maxWidth    

                | None -> // very first cog
                  let (x,y) = first
                  let current = currentCog |> Cog.addTarget x 0.             
                  let currentRadius = Cog.cogWidth * (Cog.scaleFactor currentCog.Data.Size) * 0.5
                  let totalWidth = x + currentRadius
                  let totalHeight = y + currentRadius
                  addCog (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current]) maxWidth
                
                else // we don't have enough space to fill all our cogs
                  cogs, (totalWidth,totalHeight)

            | _ -> // we have enough cogs   
              cogs, (totalWidth,totalHeight)
          

          // center our cogs
          let maxWidth = renderer.width * 0.8
          let targets,(totalWidth,totalHeight) = addCog 0 (0.,0.) (0.,0.) None [] maxWidth
          let xMargin = (renderer.width - totalWidth) * 0.5
          let yMargin = totalHeight * 0.5
          targets 
            |> Seq.map ( 
                (Cog.placeMarker xMargin yMargin (renderer.height*0.5)) 
                  >> targetsContainer.addChild
                )
             |> Seq.toArray
       
        { model with Targets = targets; State=PlaceDock }
      
      | PlaceDock -> // prepare our 4 base cogs

        let cogs = Dock.prepare dockContainer app.stage renderer 
        { model with Cogs=cogs; State=Play}

      | Play -> 

        // Animations
        if model.Score > 0 then
          turnCogs model
        
        // Events
        let updatedTargets = 
          if model.Message.IsSome then 
            let msg = model.Message.Value
            match msg  with 
              | OnMove cog -> // we want to know if we've dragged one of our cog on a target
                let pos : PIXI.Point = !!cog.position
                model.Targets
                  |> Seq.mapi( fun i target -> 
                    if not target.Data.IsFound then 
                      let position : PIXI.Point = !!target.position
                      
                      // very simple distance text
                      let a = position.x - pos.x
                      let b = position.y - pos.y
                      let distance = JS.Math.sqrt( a*a + b*b)

                      // look if we are in close vicinity of a potential target
                      let checkRadius = Cog.cogWidth * (Cog.scaleFactor cog.Data.Size) * 0.2
                      if distance < checkRadius then
                        if cog.Data.Size = target.Data.Size then 

                          // ok our cog has been placed at the right place
                          // store index for faster animation renders              
                          model.Found <- Array.append model.Found [|i|]
                          
                          // restore cog to initial position
                          Cog.onDragEnd cog ()
                          
                          // display the target cog
                          Cog.show target |> ignore                      
                          
                          // update score
                          model.Score <- model.Score + 1
                      //target
                    target
                  )
                  |> Seq.toArray
             else model.Targets

        // check if the game's finished
        if model.Score >=  model.Goal then 
          { model with State = GameOver;Targets = updatedTargets }
        else 
          { model with Targets = updatedTargets}

      | DoNothing -> model
      | GameOver -> model

// start our main loop
let start() = 
  app.ticker.add(tick) |> ignore

start() // it all begins there