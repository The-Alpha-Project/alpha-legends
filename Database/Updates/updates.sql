create table if not exists applied_updates
(
	id varchar(9) not null primary key default '000000000'
);

delimiter $
begin not atomic
	-- 10/09/2019
	if (select count(*) from applied_updates where id='100920191') = 0 then
		-- Changing displayids for the correct ones
		update item_template set displayid = 7601 where entry = 1251; -- Linen Bandage
		update item_template set displayid = 7588 where entry = 2581; -- Heavy Linen Bandage
		update item_template set displayid = 9430 where entry = 3530; -- Wool Bandage
		update item_template set displayid = 7598 where entry = 3531; -- Heavy Wool Bandage

		insert into applied_updates values ('100920191');
	end if;

	-- 12/09/2019
	if (select count(*) from applied_updates where id='120920191') = 0 then
		-- Fix broken Thunder Bluff elevators
		-- TODO: 4171 and 4173 are still a bit bugged.
		update gameobjects set entry = 4173 where entry = 4171;
		update gameobjects set entry = 4171 where entry = 47296;
		update gameobjects set entry = 4172 where entry = 47297; 

		insert into applied_updates values ('120920191');
	end if;

	-- 13/09/2019 1
	if (select count(*) from applied_updates where id='130920191') = 0 then
		-- "Ticket" system
		create table if not exists tickets
		(
			id int(11) not null primary key auto_increment,
			is_bug int(1) not null default 0,
			account_name varchar(250) not null default '',
			account_id int(10) unsigned not null default 0,
			character_name varchar(12) not null default '',
			text_body text not null default '',
			submit_time timestamp default current_timestamp on update current_timestamp,

			foreign key (account_id) references accounts(id) on delete cascade on update cascade
		);

		-- Correct stats for Small Shield
		update item_template set buyprice = 35, sellprice = 6, itemlevel = 3, armor = 29, maxdurability = 35 where entry = 2133;

		-- Using the correct alpha Small Shield
		set foreign_key_checks = 0;
		update npc_vendor set item = 2133 where item = 17184;
		update creature_loot_template set item = 2133 where item = 17184;
		set foreign_key_checks = 1;
		delete from item_template where entry = 17184;

		insert into applied_updates values ('130920191');
	end if;

	-- 13/09/2019 2
	if (select count(*) from applied_updates where id='130920192') = 0 then
		delete from spawns_gameobjects where spawn_id = 26727;
		delete from spawns_gameobjects where spawn_id = 26728;
		delete from spawns_gameobjects where spawn_id = 26740;

		insert into worldports (x, y, z, map, name) values (3167.27, -1407.15, 0, 17, 'Kalidar');
		insert into worldports (x, y, z, map, name) values (0, 0, 0, 29, 'Cage');
		insert into worldports (x, y, z, map, name) values (100, 100, 200, 30, 'PvPZone01');
		insert into worldports (x, y, z, map, name) values (277.77, -888.38, 400, 37, 'PvPZone02');
		insert into worldports (x, y, z, map, name) values (0.76, -0.91, -2.32, 44, 'OldScarletMonastery');

		insert into applied_updates values ('130920192');
	end if;

	-- 14/09/2019 1
	if (select count(*) from applied_updates where id='140920191') = 0 then
		update creature_model_info set modelid = 3831 where modelid = 10699;

		insert into applied_updates values ('140920191');
	end if;

	-- 15/09/2019 1
	if (select count(*) from applied_updates where id='150920191') = 0 then
		update creatures set modelid = 3166 where modelid = 1918;
		update creatures set modelid = 1545 where entry = 2612;
		update creatures set modelid = 1545 where entry = 2738;
		update creatures set modelid = 197, minlevel = 8, maxlevel = 8 where entry = 197;
		update creatures set minlevel = 5, maxlevel = 5 where entry = 823;
		update creatures set modelid = 164 where entry in (1642, 823);

		update spawns_creatures set spawn_positionx = -8933.54, spawn_positiony = -136.523, spawn_positionz = 83.4466, spawn_orientation = 1.97222 where spawn_id = 79970;
		update spawns_creatures set spawn_positionx = -8924.164, spawn_positiony = -136.1524, spawn_positionz = 81.0561, spawn_orientation = 2.192002 where spawn_id = 79942;

		update item_template set displayid = 292 where displayid = 15710;

		update quests set details = "Kobolds have infested the woods to the north. If you kill 7 of the grimy Vermin for me, then you'll have my thanks, citizen.", objectives = "Kill 7 Kobold Vermin, then return to Marshal McBride.", 
			ReqCreatureOrGOCount1 = 7, RewOrReqMoney = 20, RewItemId1 = 118 where entry = 7;

		insert into applied_updates values ('150920191');
	end if;

	-- 16/09/2019 1
	if (select count(*) from applied_updates where id='160920191') = 0 then
		update creatures set modelid = 164 where modelid = 3167;

		update spawns_creatures set spawn_displayid = 3831 where spawn_displayid = 10699;
		update spawns_creatures set spawn_displayid = 3166 where spawn_displayid = 1918;
		update spawns_creatures set spawn_displayid = 1545 where spawn_entry = 2612;
		update spawns_creatures set spawn_displayid = 1545 where spawn_entry = 2738;
		update spawns_creatures set spawn_displayid = 197 where spawn_entry = 197;
		update spawns_creatures set spawn_displayid = 164 where spawn_displayid = 3167;

		-- Kobold Vermins
		INSERT INTO `spawns_creatures` VALUES (79992,6,0,365,0,-8783.05,-161.565,82.0352,0.904036,180,5,0,27,0,0,1),(79993,6,0,365,0,-8774.13,-184.494,83.1764,1.59099,180,5,0,27,0,0,1),(79994,6,0,365,0,-8794.45,-170.399,81.5514,1.74907,180,5,0,27,0,0,1),(79995,6,0,365,0,-8794.96,-134.15,83.0352,2.1558,180,5,0,27,0,0,1),(79996,6,0,365,0,-8789.91,-143.313,82.7331,2.96193,180,5,0,27,0,0,1),(79998,6,0,365,0,-8768.46,-176.434,83.4446,3.31613,180,0,0,27,0,0,0),(79999,6,0,365,0,-8752.95,-160.776,84.243,2.22203,180,5,0,27,0,0,1),(80000,6,0,365,0,-8779.8,-195.355,84.0514,3.78246,180,5,0,27,0,0,1),(80001,6,0,365,0,-8775.9,-148.466,81.4102,4.05152,180,5,0,27,0,0,1),(80002,6,0,365,0,-8785.46,-171.229,81.6764,5.11032,180,5,0,27,0,0,1),(80011,6,0,365,0,-8765.28,-93.41,89.7241,0.129198,180,5,0,27,0,0,1),(80012,6,0,365,0,-8771.45,-115.916,83.365,0.383972,180,0,0,27,0,0,0),(80013,6,0,365,0,-8794.01,-118.5,83.5315,1.02964,180,5,0,27,0,0,1),(80014,6,0,365,0,-8778.78,-125.732,82.7815,1.30576,180,5,0,27,0,0,1),(80015,6,0,365,0,-8781.3,-115.552,82.8641,0.122173,180,0,0,27,0,0,0),(80016,6,0,365,0,-8766.97,-117.391,83.6927,2.30383,180,0,0,27,0,0,0),(80017,6,0,365,0,-8761.02,-127.543,83.4305,1.01327,180,5,0,27,0,0,1),(80018,6,0,365,0,-8780.05,-108.379,83.6632,5.79449,180,0,0,27,0,0,0),(80019,6,0,365,0,-8772.89,-103.59,86.1565,5.22354,180,5,0,27,0,0,1),(80024,6,0,365,0,-8749.15,-114.985,85.9305,6.13703,180,5,0,27,0,0,1),(80028,6,0,365,0,-8753.58,-192.765,85.7976,1.5708,180,0,0,27,0,0,0),(80031,6,0,365,0,-8757.55,-180.684,85.0116,1.62418,180,5,0,27,0,0,1),(80032,6,0,365,0,-8768.17,-190.502,84.5514,6.08596,180,5,0,27,0,0,1),(80094,6,0,365,0,-8760.26,-201.47,86.3046,0.963932,180,5,0,27,0,0,1),(80098,6,0,365,0,-8784.33,-245.049,83.121,3.66519,180,0,0,27,0,0,0),(80099,6,0,365,0,-8773.21,-242.694,83.9417,5.63666,180,5,0,27,0,0,1),(80100,6,0,365,0,-8765.42,-256.049,80.6917,3.86551,180,5,0,27,0,0,1),(80108,6,0,365,0,-8810.02,-217.027,83.6212,1.46425,180,5,0,27,0,0,1),(80109,6,0,365,0,-8809.45,-233.871,82.5042,4.57276,180,0,0,27,0,0,0),(80110,6,0,365,0,-8805.95,-244.942,82.4202,1.95947,180,5,0,27,0,0,1),(80114,6,0,365,0,-8806.91,-226.327,82.8712,5.20619,180,5,0,27,0,0,1),(80115,6,0,365,0,-8796.38,-247.373,82.4417,4.31741,180,5,0,27,0,0,1),(80116,6,0,365,0,-8796.15,-258.344,82.8167,2.70763,180,5,0,27,0,0,1);

		insert into applied_updates values ('160920191');
	end if;

	-- 17/09/2019 1
	if (select count(*) from applied_updates where id='170920191') = 0 then
		alter table character_inventory change item guid int(11);
		-- Gadgetzan Stove and Forge TODO: Check their orientation, they don't seem to move
		update spawns_gameobjects set spawn_positionx = -7158.355, spawn_positiony = -3731.407, spawn_positionz = 8.526073, spawn_orientation = 1.695269 where spawn_id = 17445;
		update spawns_gameobjects set spawn_positionx = -7156.319, spawn_positiony = -3725.567, spawn_positionz = 8.627331, spawn_orientation = 5.161635 where spawn_id = 17240;

		-- Squire Rowe
		delete from spawns_creatures where spawn_id = 79862;

		insert into applied_updates values ('170920191');
	end if;

	-- 17/09/2019 2
	if (select count(*) from applied_updates where id='170920192') = 0 then
		update item_template set displayid = 308 where displayid = 21905;
		update item_template set displayid = 6389 where displayid = 18060;
		update item_template set displayid = 7175 where displayid = 14994;
		update item_template set displayid = 7628 where entry = 1179;
		update item_template set displayid = 11032 where displayid = 18114 or entry = 1708;

		set foreign_key_checks = 0;
		alter table npc_vendor drop foreign key vendor_item;
		alter table npc_vendor add constraint vendor_item foreign key (item) references item_template(entry) on delete cascade on update cascade;
		set foreign_key_checks = 1;

		delete from item_template where entry in (18005, 21815, 21829, 21833);
		delete from spawns_creatures where spawn_id = 32071;

		insert into applied_updates values ('170920192');
	end if;

	-- 18/09/2019 1
	if (select count(*) from applied_updates where id='180920191') = 0 then
		update creatures set modelid = 164 where modelid = 3258;
		update spawns_creatures set spawn_displayid = 164 where spawn_displayid = 3258;

		set foreign_key_checks = 0;
		alter table spawns_creatures add constraint spawns_creatures_model	id foreign key (spawn_displayid) references creatures(modelid) on delete cascade on update cascade;
		set foreign_key_checks = 1;

		-- Eitrigg original position
		update spawns_creatures set spawn_positionx = -607.434, spawn_positiony = -4251.33, spawn_positionz = 39.0393, spawn_orientation = 3.28122 where spawn_id = 4771;
		
		-- The New Horde quest fixes
		update quests set rewmoneymaxlevel = 40, rewrepfaction1 = 76, rewrepfaction2 = 530, rewrepvalue1 = 50, rewrepvalue2 = 50 where entry = 787;


		insert into applied_updates values ('180920191');
	end if;

	-- 19/09/2019 1
	if (select count(*) from applied_updates where id='190920191') = 0 then
		-- Charis from Lion's Pride Inn
		delete from spawns_gameobjects where spawn_id in (26804, 26801, 26803, 26246, 26259, 26249, 26250, 26251, 26252, 26792, 26243);
		update creatures set modelid = 18 where entry = 66;

		update item_template set displayid = 2970, name = "Dwarven Chain Belt" where entry = 2172;
		-- Guessing this entry
		replace into item_template (entry, class, subclass, name, displayid, quality, flags, buycount, buyprice, sellprice, inventorytype, itemlevel, requiredlevel, stackable, armor, maxdurability, bonding) values (2171, 4, 2, "Dwarven Leather Belt", 7545, 1, 0, 1, 30, 6, 6, 5, 0, 1, 19, 16, 1);
		update quests set details = "Those flaming troggs have been popping up everywhere lately, just a few at first, wandering around the hills in the southeast, but now they've appeared in greater numbers.$b$bWe didn't think they were a threat--well, besides their ugly mugs!--but then, a few months ago they attacked and overran the camp to the west. We tried taking it back, but they've not been eager to part with it.$b$bWe're a bit shorthanded here, $g lad : lass;, so if you'd like to help us out, we'd be much obliged.", objectives = "Balir Frosthammer wants you to kill 5 Rockjaw Troggs and 5 Burly Rockjaw Troggs.", rewchoiceitemid1 = 2171, rewchoiceitemid2 = 2172, rewchoiceitemid3 = 0, reqcreatureorgocount1 = 5, reqcreatureorgocount2 = 5 where entry = 170;

		insert into applied_updates values ('190920191');
	end if;

	-- 21/09/2019 1
	if (select count(*) from applied_updates where id='210920191') = 0 then
		update creatures set minlevel = maxlevel and maxlevel = minlevel where minlevel > maxlevel;

		insert into applied_updates values ('210920191');
	end if;

	-- 21/09/2019 1
	if (select count(*) from applied_updates where id='230920191') = 0 then
		-- Updates from original wdb files
		update item_template set displayid = 2163 where entry = 6096;
		update item_template set displayid = 10120 where entry = 6140;
		update item_template set displayid = 9924 where entry = 1395;
		update item_template set displayid = 9929 where entry = 55;
		update item_template set displayid = 472 where entry = 35;
		update item_template set displayid = 6380 where entry = 4604;
		update item_template set displayid = 6366 where entry = 159;
		update item_template set displayid = 6366 where entry = 159;
		update item_template set displayid = 9995 where entry = 6125;
		update item_template set displayid = 2967 where entry = 3273;
		update item_template set displayid = 4533 where entry = 1369;
		update item_template set displayid = 6731 where entry = 1366;
		update item_template set displayid = 11146 where entry = 3274;
		update item_template set displayid = 11144 where entry = 3275;
		update item_template set displayid = 7952 where entry = 2652;
		update item_template set displayid = 1542 where entry = 25;
		update item_template set displayid = 2158 where entry = 2362;
		update item_template set displayid = 11143 where entry = 3270;
		update item_template set displayid = 3260 where entry = 1396;
		update item_template set displayid = 3261 where entry = 59;
		update item_template set displayid = 11145 where entry = 6060;
		update item_template set displayid = 8775 where entry = 1376;
		update item_template set displayid = 6442 where entry = 2092;
		update item_template set displayid = 10936 where entry = 3363;
		update item_template set displayid = 2217 where entry = 2654;
		update item_template set displayid = 6952 where entry = 2389;
		update item_template set displayid = 6905 where entry = 2653;
		update item_template set displayid = 11146 where entry = 3274;
		update item_template set displayid = 9944 where entry = 53;
		update item_template set displayid = 10128 where entry = 6144;
		update item_template set displayid = 9945 where entry = 52;
		update item_template set displayid = 9946 where entry = 51;
		update item_template set displayid = 5194 where entry = 36;
		update item_template set displayid = 9993 where entry = 139;
		update item_template set displayid = 9992 where entry = 140;
		update item_template set displayid = 10128 where entry = 6144;
		update item_template set displayid = 10073 where entry = 6129;
		update item_template set displayid = 3280 where entry = 1377;
		update item_template set displayid = 10933 where entry = 1380;
		update item_template set displayid = 1883 where entry = 1378;
		update item_template set displayid = 6432 where entry = 3268;
		update item_template set displayid = 3434 where entry = 3267;
		update item_template set displayid = 2215 where entry = 2656;
		update item_template set displayid = 3712 where entry = 1367;
		update item_template set displayid = 3653 where entry = 1370;
		update item_template set name='Ragged Leather Cloak', displayid = 7547 where entry = 1372;
		update item_template set displayid = 2052 where entry = 1200;
		update item_template set displayid = 2228 where entry = 2388;
		update item_template set displayid = 6686 where entry = 4865;
		update item_template set displayid = 11489 where entry = 3264;
		update item_template set displayid = 6669 where entry = 3265;
		update item_template set displayid = 3645 where entry = 3365;
		update item_template set displayid = 3442 where entry = 3272;
		update item_template set displayid = 2380 where entry = 2656;
		update item_template set displayid = 2967 where entry = 3283;
		update item_template set displayid = 6902 where entry = 2649;
		update item_template set displayid = 6903 where entry = 2650;
		update item_template set displayid = 6904 where entry = 2651;
		update item_template set displayid = 8779 where entry = 2570;
		update item_template set displayid = 11390 where entry = 3292;
		update item_template set displayid = 2185 where entry = 3834;
		update item_template set displayid = 7979 where entry = 3833;
		update item_template set displayid = 6433 where entry = 15241;
		update item_template set displayid = 2564 where entry = 1364;
		update item_template set displayid = 6902 where entry = 2387;
		update item_template set displayid = 6953 where entry = 2390;
		update item_template set displayid = 6954 where entry = 2391;
		update item_template set name='Patchwork Cloth Gloves', displayid = 2578 where entry = 1430;
		update item_template set displayid = 3279 where entry = 1374;
		update item_template set displayid = 3445 where entry = 3276;
		update item_template set displayid = 10834 where entry = 2576;
		update item_template set name='Ancestral Leggings', displayid = 11394 where entry = 3291;
		update item_template set displayid = 4995 where entry = 3277;
		update item_template set name='Small Spider Limb', displayid = 6619 where entry = 5465;
		update item_template set name='Forsaken Broadsword', displayid = 1621 where entry = 5779;
		update item_template set displayid = 6413 where entry = 1113;
		update item_template set displayid = 11382 where entry = 3288;
		update item_template set displayid = 6902 where entry = 2635;
		update item_template set displayid = 6880 where entry = 3447;
		update item_template set displayid = 6905 where entry = 2645;
		update item_template set displayid = 4788 where entry = 4560;
		update item_template set displayid = 1883 where entry = 194;
		update item_template set name='Tattered Cloth Shoes', displayid = 6318 where entry = 195;
		update item_template set displayid = 3433 where entry = 3266;
		update item_template set displayid = 2215 where entry = 2648;
		update item_template set displayid = 2217 where entry = 2646;
		update item_template set displayid = 3848 where entry = 1368;
		update item_template set displayid = 3232 where entry = 3263;
		update item_template set displayid = 9908 where entry = 3595;
		update item_template set displayid = 3899 where entry = 3596;
		update item_template set displayid = 3048 where entry = 2885;
		update item_template set displayid = 6353 where entry = 2070;
		update item_template set displayid = 11135 where entry = 4344;
		update item_template set displayid = 6875 where entry = 3437;
		update item_template set displayid = 1420 where entry = 4471;
		update item_template set displayid = 11384 where entry = 3286;
		update item_template set name='Worn Leather Cloak', displayid = 2153 where entry = 1421;
		update item_template set displayid = 2398 where entry = 15210;
		update item_template set displayid = 2633 where entry = 3650;
		update item_template set displayid = 10841 where entry = 6238;
		update item_template set name='Patchwork Cloth Belt', displayid = 4549 where entry = 3370;
		update item_template set displayid = 4603 where entry = 4343;
		update item_template set displayid = 6295 where entry = 4307;
		update item_template set name='Patchwork Cloth Vest', displayid = 8643 where entry = 1433;
		update item_template set displayid = 9911 where entry = 3606;
		update item_template set displayid = 3899 where entry = 3607;
		update item_template set displayid = 2202 where entry = 2369;
		update item_template set displayid = 7400 where entry = 2934;
		update item_template set displayid = 6682 where entry = 3300;
		update item_template set displayid = 6002 where entry = 3299;
		update item_template set displayid = 6460 where entry = 2138;
		update item_template set displayid = 2559 where entry = 2213;
		update item_template set name='Ancestral Sash', displayid = 11395 where entry = 4672;
		update item_template set displayid = 11392 where entry = 3642;
		update item_template set displayid = 11392 where entry = 3290;
		update item_template set displayid = 8441 where entry = 2644;
		update item_template set displayid = 2201 where entry = 2366;
		update item_template set name='Patchwork Cloth Boots', displayid = 1861 where entry = 1427;
		update item_template set displayid = 6296 where entry = 837;
		update item_template set displayid = 9907 where entry = 3589;
		update item_template set displayid = 1818 where entry = 838;
		update item_template set name='Heavy Weave Boots', displayid = 2166 where entry = 840;
		update item_template set displayid = 3892 where entry = 3590;
		update item_template set displayid = 2164 where entry = 711;
		update item_template set displayid = 4339 where entry = 5941;
		update item_template set displayid = 6904 where entry = 2643;
		update item_template set displayid = 11136 where entry = 2572;
		update item_template set displayid = 3740 where entry = 3323;
		update item_template set displayid = 2487 where entry = 2580;
		update item_template set displayid = 2270 where entry = 2400;
		update item_template set displayid = 6952 where entry = 2401;
		update item_template set displayid = 6954 where entry = 6063;
		update item_template set displayid = 7851 where entry = 4674;
		update item_template set displayid = 2916 where entry = 5940;

		insert into applied_updates values ('230920191');
	end if;
end $
delimiter ;
