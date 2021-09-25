function test(n)
    return n*2
end

function Clamp(val, min, max)
  if(val < min)then
    return min
  elseif(val>max)then
    return max
  end
  return val
end


Enterability = {Yes = 0, No = 1, Soon = 2 }

function OnUpdate_Door(instObj, deltaTime)
  if(deltaTime == nil) then
    return "Deltatime is also nil"
  end

  if(instObj.GetParameterBool("is_opening"))then
    instObj.ChangeParameter("openness",deltaTime * 4)
      if(instObj.GetParameterFloat("openness") >= 1) then
        instObj.SetParameter("is_opening",false)
      end
    else
      instObj.ChangeParameter("openness",deltaTime * -4)
    end

    instObj.SetParameter("openness", Clamp(instObj.GetParameterFloat("openness"),0,1))

    if(instObj.cbOnChanged != nil)then
      instObj.cbOnChanged(instObj)
    end
end

function IsEnterable_Door(instObj)
  instObj.SetParameter("is_opening", true)
  if(instObj.GetParameterFloat("openness") >= 1) then
      return Enterability.Yes
  end

  return Enterability.Soon
end

function OnUpdate_MiningBase(instObj, deltaTime)

  if (instObj.JobCount() > 0) then
      --already have a job
    return nil
  end
  
  j = Job.__new(instObj.GetJobSpotTile(), nil, InstObjActions.PlaceLooseObjects, 1.0, nil,true)
  instObj.AddJob(j)
  --[[
    {
        if (instObj.JobCount() > 0)
        {
            --already have a job
            return;
        }

        Job j = Job.__new(
            instObj.GetJobSpotTile(), null, MiningBase_JobComplete, 1f, null,true
            ) ;
        instObj.AddJob(j);
        //j.RegisterJobStoppedCB(MiningBase_JobStopped);
]]
end
