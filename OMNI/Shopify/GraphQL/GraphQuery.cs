using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify
{
    internal static class GraphQuery
    {

                    //events (first: 50) {{
                    //    edges {{
                    //        node {{
                    //            id
                    //            action
                    //            appTitle
                    //            attributeToApp
                    //            attributeToUser
                    //            createdAt
                    //            criticalAlert
                    //            message

                    //        }}

                    //    }}
                    //}}


        public static string Order(string param1)
        {
            string Query = @$" query GetOrdersUpdatedAfter
            {{
              orders({param1}) {{
               edges {{
                  node {{
                    id
                    name
                    email
                    phone
                    createdAt
                    updatedAt
                    closed
                    closedAt
                    cancelledAt
                    cancelReason
                    processedAt
                    fullyPaid
                    unpaid
                    displayFinancialStatus
                    displayFulfillmentStatus
                    edited
                    tags
                    currentSubtotalPriceSet {{
                        presentmentMoney {{
                            amount
                            currencyCode
                        }}
                        shopMoney {{
                            amount
                            currencyCode
                        }}
                    }}
                    shippingAddress {{
                      address1
                      address2
                      city
                      company
                      coordinatesValidated
                      country
                      countryCodeV2
                      firstName
                      lastName
                      formattedArea
                      id
                      latitude
                      longitude
                      name
                      phone
                      province
                      provinceCode
                      timeZone
                      zip
                    }}
                    billingAddress {{
                      address1
                      address2
                      city
                      company
                      coordinatesValidated
                      country
                      countryCodeV2
                      firstName
                      lastName
                      formattedArea
                      id
                      latitude
                      longitude
                      name
                      phone
                      province
                      provinceCode
                      timeZone
                      zip
                    }}
                    customer {{
                      addresses(first: 10) {{
                        address1
                        address2
                        city
                        company
                        coordinatesValidated
                        country
                        countryCodeV2
                        firstName
                        lastName
                        formattedArea
                        id
                        latitude
                        longitude
                        name
                        phone
                        province
                        provinceCode
                        timeZone
                        zip
                      }}
                      amountSpent {{
                        amount
                        currencyCode
                      }}
                      canDelete
                      createdAt
                      defaultAddress {{
                        address1
                        address2
                        city
                        company
                        coordinatesValidated
                        country
                        countryCodeV2
                        firstName
                        lastName
                        formatted(withName: true)
                        formattedArea
                        id
                        latitude
                        longitude
                        name
                        phone
                        province
                        provinceCode
                        timeZone
                        zip
                      }}
                      displayName
                      email
                      firstName
                      id
                      lastName
                      phone
                    }}
                    lineItems(first: 100) {{
                      edges {{
                        node {{
                          id
                          name
                          title
                          sku
                          quantity
                          currentQuantity
                          refundableQuantity
                          nonFulfillableQuantity
                          taxable
                          taxLines(first: 100) {{
                            priceSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            rate
                            ratePercentage
                            title
                          }}
                          originalUnitPriceSet {{
                            presentmentMoney {{
                              amount
                              currencyCode
                            }}
                            shopMoney {{
                              amount
                              currencyCode
                            }}
                          }}
                          discountedUnitPriceAfterAllDiscountsSet {{
                            presentmentMoney {{
                              amount
                              currencyCode
                            }}
                            shopMoney {{
                              amount
                              currencyCode
                            }}
                          }}
                          totalDiscountSet {{
                            presentmentMoney {{
                              amount
                              currencyCode
                            }}
                            shopMoney {{
                              amount
                              currencyCode
                            }}
                          }}
                          unfulfilledQuantity
                          requiresShipping
                          duties {{
                            countryCodeOfOrigin
                            id
                            price {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            taxLines {{
                              priceSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              rate
                              ratePercentage
                              title
                            }}
                          }}
                        }}
                      }}
                    }}
                    fulfillmentsCount {{
                      count
                      precision
                    }}
                    fulfillments(first: 100) {{
                      id
                      createdAt
                      updatedAt
                      deliveredAt
                      displayStatus
                      estimatedDeliveryAt
                      status
                      totalQuantity
                      trackingInfo(first:100) {{
                        company
                        number
                        url
                      }}
                      fulfillmentLineItems(first: 100) {{
                        edges {{
                          node {{
                            id
                            discountedTotalSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            lineItem {{
                              id
                              sku
                              name
                              title
                              currentQuantity
                              discountedTotalSet(withCodeDiscounts: true) {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              discountedUnitPriceAfterAllDiscountsSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              discountedUnitPriceSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              isGiftCard
                              unfulfilledQuantity
                              originalTotalSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              originalUnitPriceSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              quantity
                              refundableQuantity
                              requiresShipping
                              taxable
                              taxLines(first: 100) {{
                                priceSet {{
                                  presentmentMoney {{
                                    amount
                                    currencyCode
                                  }}
                                  shopMoney {{
                                    amount
                                    currencyCode
                                  }}
                                }}
                                rate
                                ratePercentage
                                title
                              }}
                              totalDiscountSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              unfulfilledDiscountedTotalSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              unfulfilledOriginalTotalSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              unfulfilledQuantity
                            }}
                            originalTotalSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            quantity
                          }}
                        }}
                      }}
                    }}
                    netPaymentSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    note
                    originalTotalAdditionalFeesSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    originalTotalDutiesSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    originalTotalPriceSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    paymentGatewayNames
                    presentmentCurrencyCode
                    refundable

                    requiresShipping
                    returnStatus
                    taxesIncluded
                    taxExempt
                    taxLines {{
                      priceSet {{
                        presentmentMoney {{
                          amount
                          currencyCode
                        }}
                        shopMoney {{
                          amount
                          currencyCode
                        }}
                      }}
                      rate
                      ratePercentage
                      title
                    }}
                    totalDiscountsSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalOutstandingSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalPriceSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalReceivedSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalRefundedSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalRefundedShippingSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    currentSubtotalPriceSet {{
                        presentmentMoney {{
                            amount
                            currencyCode
                        }}
                        shopMoney {{
                            amount
                            currencyCode
                        }}
                    }}
                    currentTotalPriceSet {{
                        presentmentMoney {{
                            amount
                            currencyCode
                        }}
                        shopMoney {{
                            amount
                            currencyCode
                        }}
                    }}
                    totalShippingPriceSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    totalTaxSet {{
                      presentmentMoney {{
                        amount
                        currencyCode
                      }}
                      shopMoney {{
                        amount
                        currencyCode
                      }}
                    }}
                    refunds(first: 100) {{
                      id
                      note
                      createdAt
                      duties {{
                        amountSet {{
                          presentmentMoney {{
                            amount
                            currencyCode
                          }}
                          shopMoney {{
                            amount
                            currencyCode
                          }}
                        }}
                        originalDuty {{
                          countryCodeOfOrigin
                          id
                          price {{
                            presentmentMoney {{
                              amount
                              currencyCode
                            }}
                            shopMoney {{
                              amount
                              currencyCode
                            }}
                          }}
                          taxLines {{
                            priceSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            rate
                            ratePercentage
                            title
                          }}
                        }}
                      }}
                      refundLineItems(first: 100) {{
                        edges {{
                          node {{
                            lineItem {{
                              id
                              name
                              title
                              sku

                              currentQuantity
                              discountedTotalSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              discountedUnitPriceAfterAllDiscountsSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              discountedUnitPriceSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              duties {{
                                countryCodeOfOrigin
                                id
                                price {{
                                  presentmentMoney {{
                                    amount
                                    currencyCode
                                  }}
                                  shopMoney {{
                                    amount
                                    currencyCode
                                  }}
                                }}
                                taxLines {{
                                  priceSet {{
                                    presentmentMoney {{
                                      amount
                                      currencyCode
                                    }}
                                    shopMoney {{
                                      amount
                                      currencyCode
                                    }}
                                  }}
                                  rate
                                  ratePercentage
                                  title
                                }}
                              }}

                              nonFulfillableQuantity
                              originalTotalSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              originalUnitPriceSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                              quantity
                              refundableQuantity
                              requiresShipping
                              taxable
                              taxLines(first: 100) {{
                                priceSet {{
                                  presentmentMoney {{
                                    amount
                                    currencyCode
                                  }}
                                  shopMoney {{
                                    amount
                                    currencyCode
                                  }}
                                }}
                                rate
                                ratePercentage
                                title
                              }}
                              totalDiscountSet {{
                                presentmentMoney {{
                                  amount
                                  currencyCode
                                }}
                                shopMoney {{
                                  amount
                                  currencyCode
                                }}
                              }}
                            }}
                            priceSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            quantity
                            subtotalSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            totalTaxSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                          }}
                        }}
                      }}
                      transactions(first: 100) {{
                        edges {{
                          node {{
                            id
                            amount
                            amountSet {{
                              presentmentMoney {{
                                amount
                                currencyCode
                              }}
                              shopMoney {{
                                amount
                                currencyCode
                              }}
                            }}
                            fees {{
                              amount {{
                                amount
                                currencyCode
                              }}
                              flatFee {{
                                amount
                                currencyCode
                              }}
                              flatFeeName
                              id
                              rate
                              rateName
                              taxAmount {{
                                amount
                                currencyCode
                              }}
                              type
                            }}
                            formattedGateway
                            gateway
                            processedAt
                            kind
                            status
                            createdAt
                            authorizationCode
                          }}
                        }}
                      }}
                    }}
                    transactions(first: 100) {{
                      id
                      createdAt
                      amountSet {{
                        presentmentMoney {{
                          amount
                          currencyCode
                        }}
                        shopMoney {{
                          amount
                          currencyCode
                        }}
                      }}
                      authorizationCode
                      fees {{
                        amount {{
                          amount
                          currencyCode
                        }}
                      }}
                      formattedGateway
                      gateway
                      kind
                      processedAt
                      settlementCurrency
                      settlementCurrencyRate
                      status
                    }}
                  }}
                }}
                pageInfo {{
                  hasNextPage
                  endCursor
                }}
              }}
            }}

                             ";
            return Query;
        }

        public static string Product(string param)
        {
            string Query = @$"
            {{
              products({param}) {{
                    edges {{
                  node {{
                    id
                    category {{
                      id
                      fullName
                      isLeaf
                      isRoot
                      name
                      parentId
                      childrenIds
                    }}
                    createdAt
                    description
                    descriptionHtml
                    isGiftCard
                    priceRangeV2 {{
                      maxVariantPrice {{
                        amount
                        currencyCode
                      }}
                      minVariantPrice {{
                        amount
                        currencyCode
                      }}
                    }}
                    productType
                    publishedAt
                    status
                    title
                    totalInventory
                    updatedAt
                    variantsCount {{
                      count
                      precision
                    }}
                    variants(first: 100) {{
                      edges {{
                        node {{
                          id
                          barcode
                          sku
                          availableForSale
                          compareAtPrice
                          displayName
                          title
                          createdAt
                          price
                          inventoryQuantity
                          id
                          inventoryItem {{
                          tracked
                          unitCost {{
                                amount
                                currencyCode
                            }}
                        inventoryLevels(first: 5) {{
                        edges {{
                          node {{
                            location {{
                              id
                              name
                            }}
                            quantities(names: [""available"", ""committed"", ""on_hand""]) {{
                              name
                              quantity
                            }}
                          }}
                        }}
                      }}
                          }}
                        }}
                      }}
                    }}
                  }}
                }}
                pageInfo {{
                  endCursor
                  hasNextPage
                }}
              }}
            }}

            ";

            return Query;
        }

        public static string ProductInfo(string param)
        {
            string Query = @$"
                                {{
                                  products({param}) {{
                                        edges {{
                                      node {{
                                        id
                                        status
                                        totalInventory
                                        updatedAt
                                        variants(first: 100) {{
                                          edges {{
                                            node {{
                                              id
                                              barcode
                                              sku
                                              availableForSale
                                              compareAtPrice
                                              displayName
                                              title
                                              createdAt
                                              price
                                              inventoryQuantity
                                              inventoryItem {{
                                              tracked
                                              }}
                                            }}
                                          }}
                                        }}
                                      }}
                                    }}
                                    pageInfo {{
                                      endCursor
                                      hasNextPage
                                    }}
                                  }}
                                }}

            ";

            return Query;
        }

        public static string productCount()
        {
            return @"
            query {
              productsCount {
                count
                precision
              }
            }";
        }

        public static string OrderLogAndComments()
        {

            return @"
query GetOrderTimeline($id: ID!) {
  order(id: $id) {
    id
    name
    createdAt
    hasTimelineComment
    
    events(first: 50, reverse: true) {
      edges {
        node {
          __typename
          id
          createdAt
          message

          ... on BasicEvent {
            action
          }

          ... on CommentEvent {
            rawMessage
            edited
            canEdit
            canDelete
            criticalAlert
            attachments {
              name
              url
            }
            author {
              name
              email
            }
            attributeToApp
            attributeToUser
            appTitle
          }
        }
      }
      pageInfo {
        hasNextPage
        endCursor
      }
    }
  }
}
";
        }
    }
}