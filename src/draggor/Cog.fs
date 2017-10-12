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
let cogsCount = 15

// create a balanced list of cogs sizes
let cogSizes() = 

  // Create a table of cog size using some probabilities    
  let distribution = [|
    for i in 0..(cogsCount-1) do 
      let rand = Math.random()
      yield
        match rand with 
        | x when x > 0. && x < 0.3 -> GameScreen.Cog.Tiny
        | x when x >= 0.3 && x < 0.7 -> GameScreen.Cog.Small
        | x when x >= 0.7 && x < 0.9 -> GameScreen.Cog.Medium
        | _ -> GameScreen.Cog.Large          
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
let scaleFactor (size:GameScreen.Cog.Size) = 
  match size with 
  | GameScreen.Cog.Tiny -> 0.25
  | GameScreen.Cog.Small -> 0.5
  | GameScreen.Cog.Medium -> 0.75
  | GameScreen.Cog.Large -> 1.0


let make (kind:GameScreen.Cog.Size) =

  let texture = Assets.getTexture "cog"
  if texture.IsSome then 
    
    // Extended Sprite from Fable.Pixi allows to add custom data inside our Sprite
    let data : GameScreen.Cog.CogData= 
      { IsFound = false; 
        StartPosition=(0.,0.); 
        Target=(0.,0.); 
        Size= kind; 
        IsDragging = false; 
        Interaction = None }

    let cog = 
      ExtendedSprite(texture.Value, data)
    // center the cog's anchor point
    cog.anchor.set(0.5)  

    cog
  else 
    failwith ("target is not a value")

let moveTo x y (cog:ExtendedSprite<GameScreen.Cog.CogData>) = 
  // place our cog on screen
  let position : PIXI.Point = !!cog.position
  position.x <- x
  position.y <- y
  cog.Data.StartPosition <- (x,y)
  cog

let addTarget x y (cog:ExtendedSprite<GameScreen.Cog.CogData>) = 
  // place our cog on screen
  cog.Data.Target <- (x,y) 
  cog
  
let scaleTo (cog:ExtendedSprite<GameScreen.Cog.CogData>) = 
  let scaleFactor = scaleFactor cog.Data.Size
  let scale : PIXI.Point = !!cog.scale
  scale.x <- scaleFactor
  scale.y <- scaleFactor
  cog  

// cast back to ExtendedSprite since attachEvent returns a Sprite
let castTo (sprite: PIXI.Sprite) = 
  sprite :?> ExtendedSprite<GameScreen.Cog.CogData>
  
let onDragEnd (cog:ExtendedSprite<GameScreen.Cog.CogData>) _= 
  cog.alpha <- 1.
  cog.Data.Interaction <- None
  cog.Data.IsDragging <- false
  let position : PIXI.Point = !!cog.position      
  let startX, startY = cog.Data.StartPosition
  position.x <- startX
  position.y <- startY

let onDragStart (cog: ExtendedSprite<GameScreen.Cog.CogData>) (ev:PIXI.interaction.InteractionEvent) = 
  cog.alpha <- 0.5
  cog.Data.Interaction <- Some ev.data
  cog.Data.IsDragging <- true
  
    
let onDragMove (cbk:GameScreen.Cog.Msg option->unit) stage (cog: ExtendedSprite<GameScreen.Cog.CogData>) (ev:PIXI.interaction.InteractionEvent) =
  if cog.Data.IsDragging then 
    if cog.Data.Interaction.IsSome then 
      let interaction = cog.Data.Interaction.Value
      let localPosition : PIXI.Point = interaction.getLocalPosition(stage) 
      let position : PIXI.Point = !!cog.position      
      position.x <- localPosition.x
      position.y <- localPosition.y
      cbk( Some(GameScreen.Cog.OnMove(cog,ev.data.pointerID) ))
           

// when cog is found change texture and toggle flag
let show (cog: ExtendedSprite<GameScreen.Cog.CogData>) =
  let texture = Assets.getTexture "cog"
  if texture.IsSome then 
    cog.texture <- texture.Value 
    cog.Data.IsFound <- true
    scaleTo cog
  else 
    cog

// place our markers 
let placeMarker xMargin yMargin startY layerName (cog:ExtendedSprite<GameScreen.Cog.CogData>) = 
  let (x,y) = cog.Data.Target
  let texture = Assets.getTexture "target"
  if texture.IsSome then 
    cog.texture <- texture.Value
    cog 
      |> SpriteUtils.addToLayer layerName
      |> SpriteUtils.moveTo (x + xMargin) (startY + y - yMargin)
      |> SpriteUtils.setAnchor 0.5 0.5   
      |> ignore       
  cog

// simply add our cogs
// There's really nothing complicated here
// Each cog is placed according to the previous one
// hence the recursion 
let rec fitCogInSpace (model:GameScreen.CogModel) index (totalWidth,totalHeight) first previous cogs maxWidth (sizes:GameScreen.Cog.Size [])= 
  
  match index with
  | idx when idx < model.Goal -> // check whether we can add one more cogs
    if totalWidth < maxWidth then // check wether we have enough space laeft
      let kind = sizes.[index]
      let currentCog = make kind
      match previous with 
      | Some (previousCog:ExtendedSprite<GameScreen.Cog.CogData>) ->
        
        // we want to move our new cog just next to the previous
        let (firstX,firstY) = first
        let (prevX,prevY) = previousCog.Data.Target
        let prevRadius = cogWidth * (scaleFactor previousCog.Data.Size) * 0.5
        let currentRadius = cogWidth * (scaleFactor currentCog.Data.Size) * 0.5 * 1.1
        let distance = (prevRadius + currentRadius) |> JS.Math.floor
        let angle = 340. * PIXI.Globals.DEG_TO_RAD
        let newX = prevX + JS.Math.cos(angle) * distance
        let newY = firstY + JS.Math.sin(angle) * distance
        let current = currentCog |> addTarget newX newY     
        
        let totalWidth = newX + currentRadius
        let totalHeight = newY + currentRadius
        fitCogInSpace model (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current]) maxWidth sizes    

      | None -> // very first cog
        let (x,y) = first
        let current = currentCog |> addTarget x 0.             
        let currentRadius = cogWidth * (scaleFactor currentCog.Data.Size) * 0.5
        let totalWidth = x + currentRadius
        let totalHeight = y + currentRadius
        fitCogInSpace model (idx+1) (totalWidth,totalHeight) first (Some current) (cogs @ [current]) maxWidth sizes
      
      else // we don't have enough space to fill all our cogs
        cogs, (totalWidth,totalHeight)

  | _ -> // we have enough cogs   
    cogs, (totalWidth,totalHeight)
