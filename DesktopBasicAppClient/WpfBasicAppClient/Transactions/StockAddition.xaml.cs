using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using WpfAccountClientApp.General;

namespace WpfAccountClientApp.Transactions
{
    /// <summary>
    /// Interaction logic for StockAddition.xaml
    /// </summary>
    public partial class StockAddition : Window
    {
        
        CStockAddition mStockAddition = new CStockAddition();
        ObservableCollection<CStockAdditionDetails> mGridContent = new ObservableCollection<CStockAdditionDetails>();
        String mStockAdditionID = "";
        string mBarcode = "";
        
        
        public StockAddition()
        {
            InitializeComponent();
            loadInitialDetails();
                        
        }
        

        //Member methods
        private void loadInitialDetails()
        {
            getProducts();
            newBill();            
        }

        private void newBill()
        {
            mStockAdditionID = "";
            mStockAddition = new CStockAddition();
            mTextBoxBillNo.Text = getLastBillNo();
            mDTPDate.SelectedDate = DateTime.Now;
            mTextBoxNarration.Text = ""; 
            loadFinancialCodes();            
            mComboFinancialYear.Text = CommonMethods.getFinancialCode(DateTime.Now);
            mGridContent.Clear();
            mDataGridContent.ItemsSource = mGridContent;
            clearEditBoxes();
            setGrandTotalnBalance();
        }

        private void clearEditBoxes(){
            mLabelSerialNo.Content = mDataGridContent.Items.Count+1;
            mComboProducts.Text = "";
            mComboUnits.Text = "";
            mTextBoxQuantity.Text = "";
            mTextBoxStockAdditionRate.Text = "";
            mTextBoxMRP.Text = "";                        
        }

        private void loadFinancialCodes()
        {
            try
            {
                using (ChannelFactory<IBillNo> billNoProxy = new ChannelFactory<ServerServiceInterface.IBillNo>("BillNoEndpoint"))
                {
                    billNoProxy.Open();
                    IBillNo billNoService = billNoProxy.CreateChannel();

                    List<String> fcodes = billNoService.ReadAllFinancialCodes();
                    mComboFinancialYear.ItemsSource = fcodes;                    
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private string getLastBillNo()
        {
            string billNo = "";
            try {
                using (ChannelFactory<IStockAddition> StockAdditionProxy = new ChannelFactory<ServerServiceInterface.IStockAddition>("StockAdditionEndpoint"))
                {
                    StockAdditionProxy.Open();
                    IStockAddition PurchaService = StockAdditionProxy.CreateChannel();
                    billNo=PurchaService.ReadNextBillNo(CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value)).ToString();                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return billNo;
        }

        private void getProducts()
        {
            try {
                using (ChannelFactory<IProduct> ledgerProxy = new ChannelFactory<ServerServiceInterface.IProduct>("ProductEndpoint"))
                {
                    ledgerProxy.Open();
                    IProduct ledgerService = ledgerProxy.CreateChannel();                    
                    List<CProduct> ledgers = ledgerService.ReadAllProducts();
                    mComboProducts.ItemsSource = ledgers;
                    mComboProducts.DisplayMemberPath = "Product";
                    mComboProducts.SelectedValuePath = "ProductCode";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void getUnitsOfProduct()
        {
            try
            {
                if (mComboProducts.SelectedValue != null)
                {
                    string unitCode = (mComboProducts.SelectedItem as CProduct).StockInUnitCode;

                    using (ChannelFactory<IUnit> UnitProxy = new ChannelFactory<ServerServiceInterface.IUnit>("UnitEndpoint"))
                    {
                        UnitProxy.Open();
                        IUnit UnitService = UnitProxy.CreateChannel();
                        List<CUnit> Units = UnitService.ReadSubUnits(unitCode);
                        mComboUnits.ItemsSource = Units;
                        mComboUnits.DisplayMemberPath = "Unit";
                        mComboUnits.SelectedValuePath = "UnitCode";
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }        

        private void addDataToGrid()
        {
            if (mComboProducts.SelectedIndex == -1)
            {
                MessageBox.Show("Product not given");
                mComboProducts.Focus();
                return;
            }

            if (mComboUnits.SelectedIndex == -1)
            {
                MessageBox.Show("Unit not given");
                mComboUnits.Focus();
                return;
            }

            decimal quantity = 0;
            try
            {
                
                quantity = decimal.Parse(mTextBoxQuantity.Text);

                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity not given");
                    mTextBoxQuantity.Focus();
                    return;
                }
            }
            catch
            {
                mTextBoxQuantity.Focus();
                return;
            }

            decimal purchaseRate=0;
            try
            {

                purchaseRate = decimal.Parse(mTextBoxStockAdditionRate.Text);

                if (purchaseRate < 0)
                {
                    MessageBox.Show("StockAddition rate not given");
                    mTextBoxStockAdditionRate.Focus();
                    return;
                }
            }
            catch
            {
                mTextBoxStockAdditionRate.Focus();
                return;
            }

            decimal mrp=0;
            try
            {

                mrp = decimal.Parse(mTextBoxMRP.Text);

                if (mrp < 0)
                {
                    MessageBox.Show("MRP not given");
                    mTextBoxMRP.Focus();
                    return;
                }
            }
            catch
            {
                mTextBoxMRP.Focus();
                return;
            }

            decimal total=0;
            try
            {
                total = decimal.Parse(mLabelTotal.Content.ToString());                
            }
            catch
            {
             
            }

            int serialNo = int.Parse(mLabelSerialNo.Content.ToString());
            if (serialNo <= mDataGridContent.Items.Count)
            {
                //Edit
                int index = mDataGridContent.SelectedIndex;
                mGridContent.Remove(mDataGridContent.SelectedItem as CStockAdditionDetails);
                mGridContent.Insert(index, new CStockAdditionDetails() { SerialNo = serialNo, Product = mComboProducts.Text.ToString(), ProductCode = mComboProducts.SelectedValue.ToString(), StockAdditionUnit = mComboUnits.Text.ToString(), StockAdditionUnitCode = mComboUnits.SelectedValue.ToString(), Quantity=quantity,StockAdditionRate=purchaseRate,MRP=mrp, Total=total, StockAdditionUnitValue= (mComboUnits.SelectedItem as CUnit).UnitValue  ,Barcode=mBarcode });
            }
            else
            {
                //Add
                CStockAdditionDetails crd = new CStockAdditionDetails() { SerialNo = serialNo, Product = mComboProducts.Text.ToString(), ProductCode = mComboProducts.SelectedValue.ToString(), StockAdditionUnit = mComboUnits.Text.ToString(), StockAdditionUnitCode = mComboUnits.SelectedValue.ToString(), Quantity = quantity, StockAdditionRate = purchaseRate, MRP = mrp, Total = total, StockAdditionUnitValue = (mComboUnits.SelectedItem as CUnit).UnitValue, Barcode = "" };
                mGridContent.Add(crd);
            }
            
            clearEditBoxes();
            mDataGridContent.ScrollIntoView(mDataGridContent.Items.GetItemAt(mDataGridContent.Items.Count-1));
            mComboProducts.Focus();

            setGrandTotalnBalance();
        }

        private void selectDataToEditBoxes()
        {
            if (mDataGridContent.SelectedIndex > -1)
            {
                CStockAdditionDetails crd=(CStockAdditionDetails)mDataGridContent.Items.GetItemAt(mDataGridContent.SelectedIndex);
                mLabelSerialNo.Content = crd.SerialNo;
                mComboProducts.Text = crd.Product;
                mComboUnits.Text = crd.StockAdditionUnit;
                mTextBoxQuantity.Text = crd.Quantity.ToString();
                mTextBoxStockAdditionRate.Text = crd.StockAdditionRate.ToString();
                mTextBoxMRP.Text = crd.MRP.ToString();
                mBarcode = crd.Barcode;
            }
        }

        private void removeFromGrid()
        {
            if (mDataGridContent.SelectedIndex > -1)
            {
                mGridContent.Remove(mDataGridContent.SelectedItem as CStockAdditionDetails);

                //Reseting the Serial Nos
                for(int i = 0; i < mGridContent.Count; i++)
                {
                    mGridContent.ElementAt(i).SerialNo = i + 1;                    
                }
                mDataGridContent.Items.Refresh();
                clearEditBoxes();
            }

            setGrandTotalnBalance();
        }

        private void showDataFromDatabase()
        {
            try
            {
                using (ChannelFactory<IStockAddition> StockAdditionProxy = new ChannelFactory<ServerServiceInterface.IStockAddition>("StockAdditionEndpoint"))
                {
                    StockAdditionProxy.Open();
                    IStockAddition StockAdditionervice = StockAdditionProxy.CreateChannel();

                    CStockAddition ccr= StockAdditionervice.ReadBill(mTextBoxBillNo.Text.Trim(), CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value));
                    
                    if (ccr != null)
                    {                        
                        mStockAdditionID = ccr.Id.ToString();                        
                        mDTPDate.SelectedDate = ccr.BillDateTime;
                        mTextBoxNarration.Text = ccr.Narration;
                                                 
                        mGridContent.Clear();
                        foreach (var item in ccr.Details)
                        {
                            mGridContent.Add(item);
                        }
                        mDataGridContent.Items.Refresh();
                    }                    
                }

                setGrandTotalnBalance();
                clearEditBoxes();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
     
        private void saveDataToDatabase()
        {
            try
            {                
                
                if (mDataGridContent.Items.Count==0)
                {
                    mComboProducts.Focus();
                    return;
                }

                using (ChannelFactory<IStockAddition> StockAdditionProxy = new ChannelFactory<ServerServiceInterface.IStockAddition>("StockAdditionEndpoint"))
                {
                    StockAdditionProxy.Open();
                    IStockAddition StockAdditionService = StockAdditionProxy.CreateChannel();

                    CStockAddition ccp = new CStockAddition();
                    ccp.BillNo = mTextBoxBillNo.Text.Trim();
                    ccp.BillDateTime = mDTPDate.SelectedDate.Value;
                    ccp.Narration = mTextBoxNarration.Text.Trim();
                    ccp.FinancialCode = CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value);
                    foreach (var item in mGridContent)
                    {
                        ccp.Details.Add(item);
                    }

                    bool success = false;
                    if (mStockAdditionID != "")
                    { 
                        success = StockAdditionService.UpdateBill(ccp);
                    }
                    else
                    {                    
                        success = StockAdditionService.CreateBill(ccp);
                    }

                    if (success)
                    {
                        newBill();
                    }
                    else
                    {
                        MessageBox.Show("Saving Failed");
                    }                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void deleteDataFromDatabase()
        {
            try
            {
                using (ChannelFactory<IStockAddition> StockAdditionProxy = new ChannelFactory<ServerServiceInterface.IStockAddition>("StockAdditionEndpoint"))
                {
                    StockAdditionProxy.Open();
                    IStockAddition PurchaService = StockAdditionProxy.CreateChannel();
                    
                    bool success= PurchaService.DeleteBill(mTextBoxBillNo.Text.Trim(), CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value));

                    if (success)
                    {
                        newBill();
                    }
                    else
                    {
                        MessageBox.Show("Deletion Failed");
                    }                   
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void setTotal()
        {
            decimal quantity = 0;
            try
            {
                quantity = decimal.Parse(mTextBoxQuantity.Text);
            }
            catch
            {

            }

            decimal purchaseRate = 0;
            try
            {
                purchaseRate = decimal.Parse(mTextBoxStockAdditionRate.Text);
            }
            catch
            {

            }

            mLabelTotal.Content = (quantity * purchaseRate).ToString("N2");
        }

        private void setGrandTotalnBalance()
        {
            decimal gTotal = 0;
            try
            {
                gTotal=mGridContent.Sum(x => (x.Quantity * x.StockAdditionRate));
                
            }
            catch
            {

            }            

            mLabelGrandTotal.Content = gTotal.ToString("N2");            

        }

        //Events
        private void mButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       
        private void mButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            addDataToGrid();
        }

        private void mDTPDate_LostFocus(object sender, RoutedEventArgs e)
        {
            mComboFinancialYear.Text= CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value);
        }

        private void mButtonNew_Click(object sender, RoutedEventArgs e)
        {
            newBill();
        }

        private void mButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            deleteDataFromDatabase();
        }

        private void mComboFinancialYear_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mComboFinancialYear.SelectedIndex > -1)
                {
                    int year = int.Parse(mComboFinancialYear.SelectedItem.ToString());
                    if (!mComboFinancialYear.SelectedItem.ToString().Equals(CommonMethods.getFinancialCode(mDTPDate.SelectedDate.Value)))
                    {
                        mDTPDate.SelectedDate = new DateTime(year, CommonMethods.FinancialStartDate.Month, CommonMethods.FinancialStartDate.Day);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }        

        private void mDataGridContent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectDataToEditBoxes();
        }

        private void mButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            removeFromGrid();
        }        

        private void mTextBoxBillNo_LostFocus(object sender, RoutedEventArgs e)
        {
            showDataFromDatabase();
        }

        private void mButtonSave_Click(object sender, RoutedEventArgs e)
        {
            saveDataToDatabase();
        }

        private void mTextBoxExpense_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            setGrandTotalnBalance();
        }

        private void mTextBoxDiscount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            setGrandTotalnBalance();
        }

        private void mTextBoxAdvance_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            setGrandTotalnBalance();
        }

        private void mTextBoxQuantity_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            setTotal();
        }

        private void mTextBoxStockAdditionRate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            setTotal();
        }
        
        private void mComboProducts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (mComboProducts.SelectedValue != null && mComboProducts.SelectedIndex > -1)
            {
                getUnitsOfProduct();
                mComboUnits.SelectedValue = (mComboProducts.SelectedItem as CProduct).StockInUnitCode;
            }
        }

    }
}
