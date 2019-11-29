using PvPModifier.Network.Packets;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace PvPModifier.Network.Events {
    public class SEconomyEvents {
        //Parent.WorldAccount.TransferTo(selectedAccount, amount, Journal.BankAccountTransferOptions.AnnounceToReceiver, args.Parameters[0] + " command", string.Format("SE: pay: {0} to {1} ", amount.ToString(), args.Parameters[1]))
        public static void OnPlayerDeath(object sender, PlayerDeathArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Killer.IP == e.Dead.IP) return;

            IBankAccount selectedAccount = SEconomyPlugin.Instance.GetBankAccount(e.Killer);
            SEconomyPlugin.Instance.WorldAccount.TransferTo(selectedAccount, 
                100,
                BankAccountTransferOptions.AnnounceToReceiver, 
                "PvP Kill Transaction", 
                $"PvP Kill: {e.Killer.Name} was awarded 100 RP.");
        }
    }
}
