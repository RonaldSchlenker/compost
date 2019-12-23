﻿module FluX.Blocks.Filter

open FluX.Audio
open FluX.Core
open FluX.Math

open System

type BiQuadCoeffs =
    { a0: float
      a1: float
      a2: float
      b1: float
      b2: float
      z1: float
      z2: float }

type BiQuadParams =
    { q: float
      frq: float
      gain: float }

let private biQuadCoeffsZero =
    { a0 = 0.0
      a1 = 0.0
      a2 = 0.0
      b1 = 0.0
      b2 = 0.0
      z1 = 0.0
      z2 = 0.0 }

(*
    These implementations are based on http://www.earlevel.com/main/2011/01/02/biquad-formulas/
    and on https://raw.githubusercontent.com/filoe/cscore/master/CSCore/DSP
*)

let private biQuadBase input (filterParams: BiQuadParams) (calcCoeffs: Env -> BiQuadCoeffs) =
    let f p r =
        // seed: if we are run the first time, use default values for lastParams+lastCoeffs
        let lastParams, lastCoeffs =
            match p with
            | None ->
                (filterParams, calcCoeffs r)
            | Some t -> t

        // calc the coeffs new if filter params have changed
        let coeffs =
            match lastParams = filterParams with
            | true -> lastCoeffs
            | false -> calcCoeffs r

        let o = input * coeffs.a0 + coeffs.z1
        let z1 = input * coeffs.a1 + coeffs.z2 - coeffs.b1 * o
        let z2 = input * coeffs.a2 - coeffs.b2 * o

        let newCoeffs =
            { coeffs with
                  z1 = z1
                  z2 = z2 }

        { value = o
          state = (filterParams, newCoeffs) }
    Block f


let lowPassDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let lowPass input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(pi * p.frq / float env.sampleRate)
        let norm = 1.0 / (1.0 + k / p.q + k * k)
        let a0 = k * k * norm
        let a1 = 2.0 * a0
        let a2 = a0
        let b1 = 2.0 * (k * k - 1.0) * norm
        let b2 = (1.0 - k / p.q + k * k) * norm
        { biQuadCoeffsZero with
              a0 = a0
              a1 = a1
              a2 = a2
              b1 = b1
              b2 = b2 }
    biQuadBase input p calcCoeffs


let bandPassDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let bandPass input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(pi * p.frq / float env.sampleRate)
        let norm = 1.0 / (1.0 + k / p.q + k * k)
        let a0 = k / p.q * norm
        let a1 = 0.0
        let a2 = -a0
        let b1 = 2.0 * (k * k - 1.0) * norm
        let b2 = (1.0 - k / p.q + k * k) * norm
        { biQuadCoeffsZero with
              a0 = a0
              a1 = a1
              a2 = a2
              b1 = b1
              b2 = b2 }
    biQuadBase input p calcCoeffs


let highShelfDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let highShelf input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
        let v = Math.Pow(10.0, Math.Abs(p.gain) / 20.0)
        match p.gain >= 0.0 with
        | true ->
            // boost
            let norm = 1.0 / (1.0 + sqrt2 * k + k * k)
            { biQuadCoeffsZero with
                  a0 = (v + Math.Sqrt(2.0 * v) * k + k * k) * norm
                  a1 = 2.0 * (k * k - v) * norm
                  a2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm
                  b1 = 2.0 * (k * k - 1.0) * norm
                  b2 = (1.0 - sqrt2 * k + k * k) * norm }
        | false ->
            // cut
            let norm = 1.0 / (v + Math.Sqrt(2.0 * v) * k + k * k)
            { biQuadCoeffsZero with
                  a0 = (1.0 + sqrt2 * k + k * k) * norm
                  a1 = 2.0 * (k * k - 1.0) * norm
                  a2 = (1.0 - sqrt2 * k + k * k) * norm
                  b1 = 2.0 * (k * k - v) * norm
                  b2 = (v - Math.Sqrt(2.0 * v) * k + k * k) * norm }
    biQuadBase input p calcCoeffs


let hishPassDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let highPass input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
        let norm = 1.0 / (1.0 + k / p.q + k * k)
        let a0 = norm
        let a1 = -2.0 * a0
        let a2 = a0
        let b1 = 2.0 * (k * k - 1.0) * norm
        let b2 = (1.0 - k / p.q + k * k) * norm
        { biQuadCoeffsZero with
              a0 = a0
              a1 = a1
              a2 = a2
              b1 = b1
              b2 = b2 }
    biQuadBase input p calcCoeffs


let lowShelfDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let lowShelf input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
        let v = Math.Pow(10.0, Math.Abs(p.gain) / 20.0)
        match p.gain >= 0.0 with
        | true ->
            // boost
            let norm = 1.0 / (1.0 + sqrt2 * k + k * k)
            { biQuadCoeffsZero with
                  a0 = (1.0 + Math.Sqrt(2.0 * v) * k + v * k * k) * norm
                  a1 = 2.0 * (v * k * k - 1.0) * norm
                  a2 = (1.0 - Math.Sqrt(2.0 * v) * k + v * k * k) * norm
                  b1 = 2.0 * (k * k - 1.0) * norm
                  b2 = (1.0 - sqrt2 * k + k * k) * norm }
        | false ->
            // cut
            let norm = 1.0 / (1.0 + Math.Sqrt(2.0 * v) * k + v * k * k)
            { biQuadCoeffsZero with
                  a0 = (1.0 + sqrt2 * k + k * k) * norm
                  a1 = 2.0 * (k * k - 1.0) * norm
                  a2 = (1.0 - sqrt2 * k + k * k) * norm
                  b1 = 2.0 * (v * k * k - 1.0) * norm
                  b2 = (1.0 - Math.Sqrt(2.0 * v) * k + v * k * k) * norm }
    biQuadBase input p calcCoeffs


let notchDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let notch input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
        let norm = 1.0 / (1.0 + k / p.q + k * k)
        let a0 = (1.0 + k * k) * norm
        let a1 = 2.0 * (k * k - 1.0) * norm
        let a2 = a0
        let b1 = a1
        let b2 = (1.0 - k / p.q + k * k) * norm
        { biQuadCoeffsZero with
              a0 = a0
              a1 = a1
              a2 = a2
              b1 = b1
              b2 = b2 }
    biQuadBase input p calcCoeffs


let peakDef =
    { frq = 1000.0
      q = 1.0
      gain = 0.0 }

let peak input (p: BiQuadParams) =
    let calcCoeffs (env: Env) =
        let v = Math.Pow(10.0, Math.Abs(p.gain) / 20.0)
        let k = Math.Tan(Math.PI * p.frq / float env.sampleRate)
        let l = p.q * k + k * k
        match p.gain >= 0.0 with
        | true ->
            // boost
            let norm = 1.0 / (1.0 + 1.0 / l)
            let a1 = 2.0 * (k * k - 1.0) * norm
            { biQuadCoeffsZero with
                  a0 = (1.0 + v / l) * norm
                  a1 = a1
                  a2 = (1.0 - v / l) * norm
                  b1 = a1
                  b2 = (1.0 - 1.0 / l) * norm }
        | false ->
            // cut
            let norm = 1.0 / (1.0 + v / l)
            let a1 = 2.0 * (k * k - 1.0) * norm
            { biQuadCoeffsZero with
                  a0 = (1.0 + 1.0 / l) * norm
                  a1 = a1
                  a2 = (1.0 - 1.0 / l) * norm
                  b1 = a1
                  b2 = (1.0 - v / l) * norm }
    biQuadBase input p calcCoeffs
