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
end $
delimiter ;
