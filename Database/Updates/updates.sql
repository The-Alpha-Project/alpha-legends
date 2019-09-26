delimiter $
begin not atomic
	-- 26/09/2019 2
	if (select count(*) from applied_updates where id='260920192') = 0 then
		update item_template set display_id = 926 where entry = 8766;
		update item_template set display_id = 2209 where entry = 17184;

		insert into applied_updates values('260920192');
	end if;
end $
delimiter ;

