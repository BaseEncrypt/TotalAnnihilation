$InvList[LargeForceFieldPack] = 1;
$MobileInvList[LargeForceFieldPack] = 1;
$RemoteInvList[LargeForceFieldPack] = 1;
AddItem(LargeForceFieldPack);

$CanAlwaysTeamDestroy[LargeForceField] = 1;

ItemImageData LargeForceFieldPackImage 
{	
	shapeFile = "AmmoPack";
	mountPoint = 2;
	mountOffset = { 0, -0.03, 0 };
	mass = 2.5;
	firstPerson = false;
};

ItemData LargeForceFieldPack 
{	
	description = "Large Force Field";
	shapeFile = "AmmoPack";
	className = "Backpack";
	heading = $InvHead[ihBar];
	imageType = LargeForceFieldPackImage;
	shadowDetailMask = 4;
	mass = 1.5;
	elasticity = 0.2;
	price = 1200;
	hudIcon = "deployable";
	showWeaponBar = true;
	hiliteOnActive = true;
};

function LargeForceFieldPack::deployShape(%player,%item) 
{	
	%client = Player::getClient(%player);
	%clientId = Player::getClient(%player);

		if(!$build)
		{
	if(%clientId.inArena)
	{ 
		Client::sendMessage(%client,0,"Cannot deploy in arena unless building is on. ");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return;	
	}
		}
	if(%player.outArea)
	{
		Client::sendMessage(%client,0,"can not deploy out of bounds");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return false;
	}
	if($TeamItemCount[GameBase::getTeam(%player) @ %item] >= $TeamItemMax[%item] && !$build) 
	{
		Client::sendMessage(%client,0,"Deployable Item limit reached for " @ %item.description @ "s");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return false;
	}
	if(!GameBase::getLOSInfo(%player,5)) 
	{
		Client::sendMessage(%client,0,"Deploy position out of range.");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return false;
	}
	if(Vector::dot($los::normal,"0 0 1") <= 0.7) 
	{
		Client::sendMessage(%client,0,"Can only deploy on flat surfaces");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return false;
	}

	%obj = $los::object;
	if(%obj.inmotion == true)	 
	{ 
		Client::sendMessage(%client,0,"Deploy area crappy, cannot deploy.");
		Client::sendMessage(%client,0,"~wC_BuySell.wav");
		return false;
	}
		
	%rot = GameBase::getRotation(%player);
	%objForceField = newObject("","StaticShape",LargeForceField,true);
	if(%player.repackEnergy != "")
	{
    GameBase::setDamageLevel(%objForceField, %player.repackDamage);
    GameBase::setEnergy(%objForceField, %player.repackEnergy);
    %player.repackDamage = "";
    %player.repackEnergy = "";
	}

	addToSet("MissionCleanup/deployed/Barrier", %objForceField);
	GameBase::setTeam(%objForceField,GameBase::getTeam(%player));
	GameBase::setPosition(%objForceField,$los::position);
	GameBase::setRotation(%objForceField,%rot);
	Gamebase::setMapName(%objForceField,"Force Field "@Client::getName(%client));
	Client::sendMessage(%client,0,"Force Field Deployed");
	GameBase::startFadeIn(%objForceField);
	playSound(SoundPickupBackpack,$los::position);
	playSound(ForceFieldOpen,$los::position);
	$TeamItemCount[GameBase::getTeam(%player) @ "LargeForceFieldPack"]++;
	%objForceField.deployer = %client; 
	if(!$build)
		Anni::Echo("MSG: ",%client," deployed a Large Force Field");
	return true;
}

StaticShapeData LargeForceField 
{ 
	shapeFile = "forcefield";
	debrisId = defaultDebrisLarge;
	maxDamage = 10.0;
	visibleToSensor = true;
	isTranslucent = true;
	description = "Force Field";
};

function LargeForceField::onDestroyed(%this) 
{ 
	if(!$NoCalcDamage)
		calcRadiusDamage(%this, $DebrisDamageType, 2.5, 0.05, 25, 13, 2, 0.40, 0.1, 250, 100);
	$TeamItemCount[GameBase::getTeam(%this) @ "LargeForceFieldPack"]--;
}

function LargeForceFieldPack::onMount(%player,%item) 
{	
	if($debug)
		Anni::Echo("?? EVENT mount "@ %item @" onto player "@ %player @" cl# "@ Player::getclient(%player));	

	%client = Player::getclient(%player);
	if(%client.weaponHelp && $TALT::Active == false && !(Player::getclient(%player)).isBlackOut)
	Bottomprint(%client, "<jc>Large Force Field: <f2>A large force field that may be used to cover turrets to further defend them.");	
}
