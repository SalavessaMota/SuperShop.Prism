﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SuperShop.Prism.Helpers;
using SuperShop.Prism.ItemViewModels;
using SuperShop.Prism.Models;
using SuperShop.Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuperShop.Prism.ViewModels
{
	public class ProductsPageViewModel : ViewModelBase
	{
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<ProductItemViewModel> _products;
        private bool _isRunning;
        private string _search;
        private List<ProductResponse> _myProducts;
        private DelegateCommand _searchCommand;

        public ProductsPageViewModel(
            INavigationService navigationService,
            IApiService apiService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = Languages.Products;
            LoadProductsAsync();
        }

        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(ShowProducts));

        public string Search
        {
            get => _search;
            set 
            {
                SetProperty(ref _search, value);
                ShowProducts();
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public ObservableCollection<ProductItemViewModel> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }


        private async void LoadProductsAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert(
                        Languages.Error,
                        Languages.ConnectionError, 
                        Languages.Accept);
                });
                return;                
            }  
            
            IsRunning = true;

            string url = App.Current.Resources["UrlAPI"].ToString();

            Response response = await _apiService.GetListAsync<ProductResponse>(url, "/api", "/Products");

            IsRunning = false;

            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error, 
                    response.Message, 
                    Languages.Accept);
                return;
            }

            _myProducts = (List<ProductResponse>)response.Result;
            ShowProducts();
        }


        private void ShowProducts()
        {
            if (string.IsNullOrEmpty(Search))
            {
                Products = new ObservableCollection<ProductItemViewModel>(_myProducts.Select(p => new ProductItemViewModel(_navigationService){
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    LastPurchase = p.LastPurchase,
                    LastSale = p.LastSale,
                    IsAvailable = p.IsAvailable,
                    Stock = p.Stock,
                    User = p.User,
                    ImageFullPath = p.ImageFullPath
                }).ToList());
            }
            else
            {
                Products = new ObservableCollection<ProductItemViewModel>(
                    _myProducts.Select(P =>
                    new ProductItemViewModel(_navigationService)
                    {
                        Id = P.Id,
                        Name = P.Name,
                        Price = P.Price,
                        ImageUrl = P.ImageUrl,
                        LastPurchase = P.LastPurchase,
                        LastSale = P.LastSale,
                        IsAvailable = P.IsAvailable,
                        Stock = P.Stock,
                        User = P.User,
                        ImageFullPath = P.ImageFullPath
                    })
                    .Where(p => p.Name.ToLower().Contains(Search.ToLower()))
                    .ToList());
            }
        }
    }
}
