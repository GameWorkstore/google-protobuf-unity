#!/bin/bash

READMECMP=`cmp README.md Assets/Package/README.md`
if [ -n "$READMECMP" ]
then
    echo "they are different"
else
    echo "they are same"
fi

READMECMP=`cmp README.md README.md`
if [ -n "$READMECMP" ]
then
    echo "they are different"
else
    echo "they are same"
fi