module Types 

open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi

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
  mutable IsFound: bool
}

type Msg =
  | OnMove of ExtendedSprite<CogData>

type State = 
  | Init
  | PlaceCogs
  | PlaceDock
  | Play 
  | GameOver 
  | DoNothing

type Model = {
  Cogs : ExtendedSprite<CogData> []
  Targets : ExtendedSprite<CogData> []
  mutable Message : Msg option
  Score : int
  Goal : int
  State : State
  Found: int []
  Emitters : PIXI.particles.Emitter []
}

let mutable model = { 
    Cogs = [||] 
    Targets = [||]
    Message = None
    Score= 0
    Goal=0
    State=Init
    Found=[||]
    Emitters=[||]
  }

[<RequireQualifiedAccess>]
module Assets = 
  let mutable textures = Map.empty<string,PIXI.Texture> 
  let mutable objFiles = Map.empty<string,obj> 

  let addTexture name texture = 
    textures <- textures.Add(name,texture) 

  let addObj name text = 
    objFiles <- objFiles.Add(name,text)     

  let getTexture name = 
     textures.TryFind name

  let getObj name = 
     objFiles.TryFind name     