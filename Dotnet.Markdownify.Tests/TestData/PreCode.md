
```
# setup our HNSW parameters
d = 128  # vector size
M = 32

index = faiss.IndexHNSWFlat(d, M)
print(index.hnsw)
```
