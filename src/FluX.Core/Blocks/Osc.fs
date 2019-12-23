﻿module FluX.Blocks.Osc

open System

open FluX.Math
open FluX.Audio
open FluX.Core

let noise() =
    let f (p: Random) r =
        let v = p.NextDouble()
        { value = v
          state = p }
    f
    |> liftSeed (new Random())
    |> Block

// TODO
// static calculation result in strange effects when modulating :D
//let sin (frq:float<Hz>) (phase:float<Deg>) =
//    let f (env:Env) =
//        let rad = env.samplePos / env.sampleRate
//        Math.Sin(rad * pi2 * (float frq))
//    build (lift_s f) ()

let private osc (frq: float) f =
    let f angle (env: Env) =
        let newAngle = (angle + pi2 * frq / (float env.sampleRate)) % pi2
        { value = f newAngle
          state = newAngle }
    f
    |> liftSeed 0.0
    |> Block

// TODO: phase
let sin (frq: float) = osc frq Math.Sin
let saw (frq: float) = osc frq (fun angle -> 1.0 - (1.0 / pi * angle))

let tri (frq: float) =
    osc frq (fun angle ->
        if angle < pi then -1.0 + (2.0 / pi) * angle
        else 3.0 - (2.0 / pi) * angle)

let square (frq: float) =
    osc frq (fun angle ->
        if angle < pi then 1.0
        else -1.0)
