module Cog 

open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.JS
open Fable.Pixi
open Types

[<Literal>]
let cogWidth = 100.

[<Literal>]
let rotation = 0.1

[<Literal>]
let cogsCount = 20

let texture = PIXI.Texture.fromImage("../img/cog.png")

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
  let cog = ExtendedSprite(texture, { IsFound = false; StartPosition=(0.,0.); Target=(0.,0.); Size= kind; IsDragging = false; Interaction = None })

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
  
let scaleTo (cog:ExtendedSprite<CogData>) = 
  let scaleFactor = scaleFactor cog.Data.Size
  let scale : PIXI.Point = !!cog.scale
  scale.x <- scaleFactor
  scale.y <- scaleFactor
  cog  

// cast back to ExtendedSprite since attachEvent returns a Sprite
let backTo (sprite: PIXI.Sprite) = 
  sprite :?> ExtendedSprite<CogData>
  
let onDragEnd (cog:ExtendedSprite<CogData>) _= 
  cog.alpha <- 1.
  cog.Data.Interaction <- None
  cog.Data.IsDragging <- false
  let position : PIXI.Point = !!cog.position      
  let startX, startY = cog.Data.StartPosition
  position.x <- startX
  position.y <- startY
  model.Message <- None   

let onDragStart  (cog: ExtendedSprite<CogData>) (ev:PIXI.interaction.InteractionEvent) = 
  cog.alpha <- 0.5
  cog.Data.Interaction <- Some ev.data
  cog.Data.IsDragging <- true
    
let onDragMove stage (cog: ExtendedSprite<CogData>) _ =
  if cog.Data.IsDragging then 
    if cog.Data.Interaction.IsSome then 
      let interaction = cog.Data.Interaction.Value
      let localPosition : PIXI.Point = interaction.getLocalPosition(stage) 
      let position : PIXI.Point = !!cog.position      
      position.x <- localPosition.x
      position.y <- localPosition.y
      model.Message <- Some (OnMove cog)
           

// when cog is found change texture and toggle flag
let show (cog: ExtendedSprite<CogData>) =
  cog.texture <- texture 
  cog.Data.IsFound <- true
  scaleTo cog

// place our markers 
let placeMarker xMargin yMargin startY (cog:ExtendedSprite<CogData>) = 
  let (x,y) = cog.Data.Target
  cog.texture <-  PIXI.Texture.fromImage("../img/target.png")
  // center the cog"s anchor point
  cog.anchor.set(0.5)  
  let position : PIXI.Point = !!cog.position
  position.x <- x + xMargin // center our cogs on x axis
  position.y <- startY + y - yMargin // center our cogs on y axis
  cog

