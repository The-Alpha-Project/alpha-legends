delimiter $
begin not atomic
	-- 26/09/2019
	if (select count(*) from applied_updates where id='260920191') = 0 then
		
	end if;
end $
delimiter ;

