environment = {}

-- Reset package path
package.path = GameDir .. "/lua/?.lua"

-- Note: sandbox has been customized to remove math.random
local sandbox = require('sandbox')
local stp = require('stacktraceplus')

local PrintStackTrace = function(msg)
	return stp.stacktrace("", 2) .. "\nError message\n===============\n" .. msg .. "\n==============="
end

debug.traceback = PrintStackTrace

local function merge(dest, source)
  for k,v in pairs(source) do
    dest[k] = dest[k] or v
  end
  return dest
end

TryRunSandboxed = function(fn, ...)
	--local success, err = xpcall(function() return sandbox.run(fn, {env = environment, quota = MaxUserScriptInstructions}, arg) end, PrintStackTrace)
	--if not success then
		--FatalError(err)
	--end
    --return err -- Actually, the correct result on success.
	environment = merge(environment, _G)
	setfenv(fn, environment)
	local success, err = xpcall(function()
		return fn(arg)
	end, PrintStackTrace)
	if not success then error(err) end
	return err
end

WorldLoaded = function()
	if environment.WorldLoaded ~= nil then
		TryRunSandboxed(environment.WorldLoaded)
	end
end

ActivateAI = function(faction_name, internal_name)
	if environment.ActivateAI ~= nil then
		TryRunSandboxed(environment.ActivateAI, faction_name, internal_name)
	end
end

Tick = function()
	if environment.Tick ~= nil then
		TryRunSandboxed(environment.Tick)
	end
end

BB_choose_building_to_build = function(params)
	if environment.BB_choose_building_to_build ~= nil then
		return TryRunSandboxed(environment.BB_choose_building_to_build, params)
	end
end

ExecuteSandboxedScript = function(file, contents)
	local script, err = loadstring(contents, file)
	if (script == nil) then
		FatalError("Error parsing " .. file .. ". Reason: " .. err)
	else
		TryRunSandboxed(script)
	end
end

RegisterSandboxedGlobal = function(key, value)
	environment[key] = value
end
