# Ferret Food Solver

This is a web API that uses optimization to find the right combination
of ingredients to provide a balanced meal.

## Dependencies

Requires GNU Linear Programming Kit to be installed and visible in path.

## Roadmap

- [x] Verification of parameters
- [x] Solving using Linear Programming
- [ ] Verify all parameters at once
- [ ] Api Usage matches expected result

## API Usage

Expected usage of the api:

    POST /solve HTTP/1.1
    Content-Length: ...
    Content-Type: application/json

    { "targetWeight": float
    , "targetMusclePercent": float
    , "targetOrganPercent": float
    , "targetHeartPercent": float
    , "targetBonePercent": float
    , "sigma": float
    , "ingredients": 
        [ { "description": string
        , "weight": float
        , "musclePercent": float
        , "organPercent": float
        , "heartPercent": float
        , "bonePercent": float
        }
        , 
        ...
        ]
    }

## API Response

If an assumption is invalidated, there will be a list of all invalidated
assumptions with a message and corresponding values.

Every key-value pair is optional, so there will only exist assumptions
that are invalidated.

    HTTP/1.1 400 Bad Request
    Content-Length: ...
    Content-Type: application/json

    { "invalidAssumptions": 
        { "targetWeight": 
            { "value": -1
            , "message": "Target Weight must be greater than 0!"
            }
        , "targetPercents": 
            { "sum": ###
            , "muscle": ###
            , "organ": ###
            , "heart": ###
            , "bone": ###
            , "message": "Target percentages must be valid!"
            }
        , "sigma": 
            { "value": -0.01
            , "message": "The sigma must be positive!"
            }
        , "ingredients":
            [ { "index": integer Nth ingredient that was passed in as a parameter
            , "description": description identifier
            , "targetWeight": 
                { "value": -1
                , "message": "Target Weight must be greater than 0!"
                }
            , "percents": 
                { "sum": ###
                , "muscle": ###
                , "organ": ###
                , "heart": ###
                , "bone": ###
                , "message": "Target percentages must be valid!"
                }
            }
        ,
        ...
        ]
        },
        "unfeasibleModel": 
        { "message": "Gurobi Error Message"
        , "status": "UNBOUNDED" | "INF_OR_UNBOUNDED"
        }
    }

A valid response will look like:

    HTTP/1.1 200 OK
    Content-Length: ...
    Content-Type: application/json

    { "actualWeight": ###
    , "actualMusclePercent": ###
    , "actualOrganPercent": ###
    , "actualHeartPercent": ###
    , "actualBonePercent": ###
    , "ingredients":
        [ { "description": description identifier 
            , "optimalNumber": ### (optimal number)
            }
        ,
        ...
        ]
    }

where ingredients should be in the order that was given.

## Notation

- $X$: set of ingredients
- $i$: ingredient $i$ such that $i \in X$
- $W'$: Target weight
- $M'$: Target muscle percent
- $O'$: Target organ percent
- $H'$: Target heart percent
- $B'$: Target bone percent
- $\sigma$: Constant for acceptable deviation.
- $x_i$: Ingredient $i$
- $W_i$: Weight of ingredient $i$
- $M_i$: Muscle percent of ingredient $i$
- $O_i$: Organ percent of ingredient $i$
- $H_i$: Heart percent of ingredient $i$
- $B_i$: Bone percent of ingredient $i$

## Formulation

$$
\begin{aligned}
\max_{x_i \in \{0,1\}} &\sum_{i \in X} W_i \times x_i\\
\mbox{s.t. }& \sum_{i \in X} W_i \times x_i \le W'\\
&\sum_{i \in X} M_i \times x_i = M' \pm \sigma\\
&\sum_{i \in X} O_i \times x_i = O' \pm \sigma\\
&\sum_{i \in X} H_i \times x_i = H' \pm \sigma\\
&\sum_{i \in X} B_i \times x_i = B' \pm \sigma\\
\end{aligned}
$$

> Let us maximize the combined weight of our ingredients,\
> such that we don't go over the maximum weight,\
> our actual muscle percent is within an acceptable deviation of our
> target muscle percent,\
> our actual organ percent is within an acceptable deviation of our
> target organ percent,\
> our actual heart percent is within an acceptable deviation of our
> target heart percent,\
> and our actual bone percent is within an acceptable deviation of our
> target bone percent.

## Assumptions

- $W' > 0$: The target weight must be valid
- $M' + O' + H' + B' \approx 100\%$: The percentages must be valid
- $\sigma \ge 0$: The sigma must be positive
- $W_i > 0 \quad \forall i \in X$: The weight of each ingredient must
  be valid
- $M_i + O_i + H_i + B_i \approx 100\% \quad \forall i \in X$: The
  percentages of each ingredient must be valid
