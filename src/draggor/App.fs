module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS
open Fable.Pixi


(* ------------------------ COGS ---------------------*)

[<Literal>]
let cogWidth = 100.

[<Literal>]
let cogsCount = 20

module Cog =

  let texture = PIXI.Texture.fromImage("../img/cog.png")

  type Size = 
    | Tiny 
    | Small
    | Medium 
    | Large

  type CogData = {
    Size: Size
    mutable Target: float*float
    mutable StartPosition: float*float
    mutable IsDragging: bool
    mutable Interaction : PIXI.interaction.InteractionData option    
  }

  // create a balanced list of cogs sizes
  let cogSizes = 

    // Create a table of cog size using some probabilities    
    let distribution = [|
      for i in 0..cogsCount do 
        let rand = Math.random()
        yield
          match rand with 
          | x when x > 0. && x < 0.3 -> Tiny
          | x when x >= 0.3 && x < 0.7 -> Small
          | x when x >= 0.7 && x < 0.9 -> Medium
          | _ -> Large          
    |]

    // shuffle our table
    let rand = new System.Random()
    let swap (a: _[]) x y =
      let tmp = a.[x]
      a.[x] <- a.[y]
      a.[y] <- tmp

    // shuffle an array (in-place)
    let al = distribution.Length
    let shuffle a =
      List.iteri (fun i _ -> swap a i (rand.Next(i, al))) !!a
    
    shuffle distribution 
    distribution

    // scale cog according to kind
  let scaleFactor size = 
    match size with 
    | Tiny -> 0.25
    | Small -> 0.5
    | Medium -> 0.75
    | Large -> 1.0


  let make kind =

    // Extended Sprite from Fable.Pixi allows to add custom data inside our Sprite
    let cog = ExtendedSprite(texture, { StartPosition=(0.,0.); Target=(0.,0.); Size= kind; IsDragging = false; Interaction = None })

    // scale our cog according to its kind
    let scaleFactor = scaleFactor cog.Data.Size
    let scale : PIXI.Point = !!cog.scale
    scale.x <- scaleFactor
    scale.y <- scaleFactor

    // center the cog's anchor point
    cog.anchor.set(0.5)  
    cog

  let moveTo x y (cog:ExtendedSprite<CogData>) = 
    // place our cog on screen
    let position : PIXI.Point = !!cog.position
    position.x <- x
    position.y <- y
    cog.Data.StartPosition <- (x,y)
    cog

  let addTarget x y (cog:ExtendedSprite<CogData>) = 
    // place our cog on screen
    cog.Data.Target <- (x,y) 
    cog
    
  // cast back to ExtendedSprite since attachEvent returns a Sprite
  let backTo (sprite: PIXI.Sprite) = 
    sprite :?> ExtendedSprite<CogData>
    
  let toInteractive  (targets:ExtendedSprite<CogData> list) (cog:ExtendedSprite<CogData>)= 

    let onDragStart (cog: ExtendedSprite<CogData>) (ev:PIXI.interaction.InteractionEvent) = 
      cog.alpha <- 0.5
      cog.Data.Interaction <- Some ev.data
      cog.Data.IsDragging <- true
        
    let onDragEnd (cog: ExtendedSprite<CogData>) _ =
      cog.alpha <- 1.
      cog.Data.Interaction <- None
      cog.Data.IsDragging <- false
      let position : PIXI.Point = !!cog.position      
      let startX, startY = cog.Data.StartPosition
      position.x <- startX
      position.y <- startY

    let onDragMove (cog: ExtendedSprite<CogData>) (targets:ExtendedSprite<CogData> list) _ =
      if cog.Data.IsDragging then 
        if cog.Data.Interaction.IsSome then 
          let interaction = cog.Data.Interaction.Value
          let localPosition : PIXI.Point = interaction.getLocalPosition(cog.parent) 
          let position : PIXI.Point = !!cog.position      
          position.x <- localPosition.x
          position.y <- localPosition.y

          // WIP
          let found = 
            targets 
              |> Seq.exists( fun target ->
                let pos : PIXI.Point = !!target.position
                let a = pos.x-position.x 
                let b = pos.y-position.y
                let distance = Math.sqrt(a*a + b*b)
                distance < 100.
              )
          
          if found then printfn "found"

      // enable the cog to be interactive... this will allow it to respond to mouse and touch events
    cog.interactive <- true
      // this button mode will mean the hand cursor appears when you roll over the cog with your mouse
    cog.buttonMode <- true

    // attach our custom events to enable dragging
    cog
      |> attachEvent Pointerdown (onDragStart cog)
      |> attachEvent Pointerup (onDragEnd cog)
      |> attachEvent Pointermove (onDragMove cog targets)
      |> backTo

    
(* ------------------------ GAME ---------------------*)


type Model = {
  Cogs : ExtendedSprite<Cog.CogData> list
}

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
  
let max = Cog.cogSizes.Length  
let startX = 0.
let startY = 0.
let maxWidth = renderer.width * 0.8

let rec addCog index (totalWidth,totalHeight) first previous cogs = 
  
  match index with
  | idx when idx < max -> // check whether we can add one more cogs
    if totalWidth < maxWidth then // check wether we have enough space laeft
      let kind = Cog.cogSizes.[index]
      let currentCog = Cog.make kind
      match previous with 
      | Some (previousCog:ExtendedSprite<Cog.CogData>) ->
        
        // we want to move our new cog just next to the previous
        let (firstX,firstY) = first
        let (prevX,prevY) = previousCog.Data.Target
        let prevRadius = cogWidth * (Cog.scaleFactor previousCog.Data.Size) * 0.5
        let currentRadius = cogWidth * (Cog.scaleFactor currentCog.Data.Size) * 0.5 * 1.1
        let distance = (prevRadius + currentRadius) |> Math.floor
        let angle = 340. * PIXI.Globals.DEG_TO_RAD
        let newX = prevX + Math.cos(angle) * distance
        let newY = firstY + Math.sin(angle) * distance
        let current = currentCog |> Cog.addTarget newX newY     
        
        let totalWidth = newX + currentRadius
        let totalHeight = newY + currentRadius
        addCog (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current])    

      | None -> // very first cog
        let (x,y) = first
        let current = currentCog |> Cog.addTarget x 0.             
        let currentRadius = cogWidth * (Cog.scaleFactor currentCog.Data.Size) * 0.5
        let totalWidth = x + currentRadius
        let totalHeight = y + currentRadius
        addCog (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current])
      
      else // we don't have enough space to fill all our cogs
        cogs, (totalWidth,totalHeight)

  | _ -> // we have enough cogs   
    cogs, (totalWidth,totalHeight)

// build our cogs list
let targets,(totalWidth,totalHeight) = addCog 0 (0.,0.) (startX,startY) None []

// place our markers 
let addMarker xMargin yMargin startY (cog:ExtendedSprite<Cog.CogData>) = 
  let (x,y) = cog.Data.Target
  let target = PIXI.Sprite.fromImage("../img/target.png")
  // center the cog"s anchor point
  target.anchor.set(0.5)  
  let position : PIXI.Point = !!target.position
  position.x <- x + xMargin // center our cogs on x axis
  position.y <- startY + y - yMargin // center our cogs on y axis
  targetsContainer.addChild target |> ignore
  cog

// place our cogs on top
let addCogs xMargin yMargin startY (cog:ExtendedSprite<Cog.CogData>) = 
  let (x,y) = cog.Data.Target
  let position : PIXI.Point = !!cog.position
  position.x <- x + xMargin // center our cogs on x axis
  position.y <- startY + y - yMargin // center our cogs on y axis
  cogsContainer.addChild cog 

// center our markers
let xMargin = (renderer.width - totalWidth) * 0.5
let yMargin = totalHeight * 0.5
targets 
  |> List.map (addMarker xMargin yMargin (renderer.height*0.5))
//  |> List.map (addCogs xMargin yMargin (renderer.height*0.5) )
  |> ignore

let model = { Cogs=targets }

module Dock = 

  // dragging cogs layer
  let dockContainer = PIXI.Container()
  app.stage.addChild dockContainer |> ignore

  // create our cogs for our dock 
  let dockY = renderer.height * 0.9
  let rec makeCogs cogs totalWidth (list:ExtendedSprite<Cog.CogData> list) targets= 
    match cogs with
    | [] -> // center our cogs on screen
      let margin = (renderer.width - totalWidth) * 0.5
      list
        |> List.map( fun cog -> 
          let (x,y) = cog.Data.StartPosition 
          (Cog.moveTo (x+margin) y >> Cog.backTo) cog
        )

    | head :: remains -> // add next cog to the dock
      let margin = 30.
      let width = (Cog.scaleFactor head) * cogWidth + margin
      let cog = 
        Cog.make head 
          |> Cog.moveTo totalWidth dockY
          |> Cog.toInteractive targets
          |> dockContainer.addChild
          |> Cog.backTo    
      makeCogs remains (totalWidth+width) (list @ [cog]) targets

  let prepare targets = 
    makeCogs 
      [
        Cog.Tiny
        Cog.Small
        Cog.Medium
        Cog.Large
      ]
      0.
      []
      targets

let dock = Dock.prepare targets

(*
// our render loop
let tick delta =
  let l = cogsContainer.children.Count
  for i in 0..l do 
    let currenTarget = cogsContainer.children.[i]
    currenTarget.rotation <- currenTarget.rotation + 0.1
  
app.ticker.add(tick) |> ignore
*)