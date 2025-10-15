using TransactionLib;

public class TransactionAnalyzerTests
{
    [Fact]
    public void RejectsUnknownTransactionType()
    {
        var tx = new Transaction
        {
            Amount = 100,
            Kind = TransactionKind.Unknown,
            Timestamp = DateTime.Now
        };

        Assert.StartsWith("������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsTooManyTransactionsForNonVip()
    {
        var tx = new Transaction
        {
            Amount = 100,
            Kind = TransactionKind.Deposit,
            DailyTransactionCount = 15,
            IsVipClient = false
        };

        Assert.Contains("��������� ����������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void AllowsVipToTransferLargeExternal()
    {
        var tx = new Transaction
        {
            Amount = 2_000_000,
            Kind = TransactionKind.Transfer,
            IsInternal = false,
            IsVipClient = true
        };

        Assert.Equal("���������� ���������.", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void AppliesCommissionForNonVipCurrentToSaving()
    {
        var tx = new Transaction
        {
            Amount = 1000,
            Kind = TransactionKind.Transfer,
            FromAccountType = "�������",
            ToAccountType = "��������������",
            IsInternal = false,
            IsVipClient = false
        };

        Assert.StartsWith("��������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsDailySumExceededForNonVip()
    {
        var tx = new Transaction
        {
            Amount = 300_000,
            Kind = TransactionKind.Transfer,
            DailyTransactionTotal = 250_000,
            IsVipClient = false
        };

        Assert.StartsWith("�����������: ���������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsLargeAtmTransfer()
    {
        var tx = new Transaction
        {
            Amount = 150_000,
            Kind = TransactionKind.Transfer,
            Channel = "ATM",
            IsInternal = true
        };

        Assert.StartsWith("�����������: ���������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void AllowsVipLargeExternalWithdrawal()
    {
        var tx = new Transaction
        {
            Amount = 500_000,
            Kind = TransactionKind.Withdrawal,
            IsInternal = false,
            IsVipClient = true,
            Channel = "Office"
        };

        Assert.Equal("���������� ���������.", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsZeroAmount()
    {
        var tx = new Transaction
        {
            Amount = 0,
            Kind = TransactionKind.Unknown,
            IsVipClient = false,
            Channel = "Web",
            Timestamp = DateTime.Now
        };

        Assert.StartsWith("������", TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    // �������������� �����

    [Fact]
    public void RejectsWebWithdrawal()
    {
        var tx = new Transaction
        {
            Amount = 100,
            Kind = TransactionKind.Withdrawal,
            Channel = "Web"
        };

        Assert.Contains("������ ����� ���-������� ���������",
            TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsWeekendOnlineTransferBetweenDifferentAccounts()
    {
        var weekend = new DateTime(2024, 3, 2); 
        Console.WriteLine($"Day of week: {weekend.DayOfWeek}"); 

        var tx = new Transaction
        {
            Amount = 1000,
            Kind = TransactionKind.Transfer,
            FromAccountType = "Current",
            ToAccountType = "Savings", 
            IsInternal = true, 
            IsVipClient = true, 
            Channel = "Web", 
            Timestamp = weekend,
            DailyTransactionCount = 1, 
            DailyTransactionTotal = 1000 
        };

        var result = TransactionAnalyzer.AnalyzeTransaction(tx);
        Assert.Contains("� �������� ������ ����������", result);
    }

    [Fact]
    public void AllowsWeekendTransferBetweenSameAccounts()
    {
        var weekend = new DateTime(2024, 3, 2); // �������
        var tx = new Transaction
        {
            Amount = 1000,
            Kind = TransactionKind.Transfer,
            FromAccountType = "�������",
            ToAccountType = "�������",
            Channel = "Web",
            Timestamp = weekend
        };

        Assert.Equal("���������� ���������.",
            TransactionAnalyzer.AnalyzeTransaction(tx));
    }

    [Fact]
    public void RejectsLargeExternalWithdrawalForNonVip()
    {
        var tx = new Transaction
        {
            Amount = 250_000,
            Kind = TransactionKind.Withdrawal,
            IsInternal = false,
            IsVipClient = false
        };

        Assert.Contains("������� ������ ������ ��� VIP-��������",
            TransactionAnalyzer.AnalyzeTransaction(tx));
    }

}
