BEGIN TRANSACTION;

CREATE TEMPORARY TABLE temp_wallet_balances
(
   WalletId VARCHAR(80),
   Symbol VARCHAR(80),
   NewBalance DECIMAL
) ON COMMIT DROP;

insert into temp_wallet_balances
select "WalletId", "Symbol", "NewBalance" from
    (
        select *, ROW_NUMBER() OVER (PARTITION BY "WalletId", "Symbol" ORDER BY "Timestamp" DESC) "rank"
        from balancehistory.balance_history
        order by "Timestamp" desc
    ) t
where t.rank = 1;

delete from liquidity_dwhdata.balancedashboard
where "BalanceDate" = (CURRENT_DATE);

insert into liquidity_dwhdata.balancedashboard ("Asset", "LastUpdateDate", "BalanceDate", "ClientBalance", "BrokerBalance", "Commission")
select
    ass.Symbol as Asset,
    (current_timestamp at time zone 'utc') as LastUpdateDate,
    (CURRENT_DATE) as BalanceDate,
    coalesce(sum(bext.NewBalance), 0) as ClientBalance,
    coalesce(sum(bint.NewBalance), 0) as BrokerBalance,
    0 as Commission
from temp_wallet_balances as ass
         left join clientwallets.wallets as w
                   on ass.WalletId = w."WalletId"
         left join temp_wallet_balances as bext
                   on bext.WalletId = ass.WalletId
                       and bext.Symbol = ass.Symbol
                       and w."IsInternal" = false
         left join temp_wallet_balances as bint
                   on bint.WalletId = ass.WalletId
                       and bint.Symbol = ass.Symbol
                       and w."IsInternal" = true
group by ass.Symbol;

COMMIT;