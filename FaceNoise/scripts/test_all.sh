# NOTE: Put the 0 before the decimal in the argument
# ./scripts/test_all.sh 0.33

rm -rf output/$1
mkdir output/$1

echo "1. DETECTING/PERTURBING FACES."
for file in input/*.jpg; do
	basefile=`basename "$file"`
	./bin/Debug/FaceNoise.exe -e $basefile $1
done

echo "2. RECGONIZING FACES."
python scripts/fb_endpoint.py < tokens.txt $1 

